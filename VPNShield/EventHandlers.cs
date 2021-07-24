using Exiled.API.Features;
using Exiled.Events.EventArgs;
using LiteNetLib.Utils;
using NorthwoodLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VPNShield
{
    public class EventHandlers
    {
        private readonly Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        private const byte BypassFlags = (1 << 1) | (1 << 3); //IgnoreBans or IgnoreGeoblock

        internal static readonly Stopwatch stopwatch = new();
        private static readonly Stopwatch cleanupStopwatch = new();
        private static readonly HashSet<PlayerToKick> ToKick = new();
        private static readonly HashSet<PlayerToKick> ToClear = new();
        private static readonly NetDataWriter writer = new();

        public void PreAuthenticating(PreAuthenticatingEventArgs ev)
        {
            if (GlobalWhitelist.GlobalWhitelistCheck(ev.UserId))
            {
                if (plugin.Config.VerboseMode)
                    Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is whitelisted from VPN and account age checks. Skipping checks.");
                return;
            }

            if (ev.UserId.Contains("@northwood", StringComparison.InvariantCultureIgnoreCase))
            { 
                if (plugin.Config.VerboseMode)
                    Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is a Northwood Studios member. Skipping checks.");
                return;
            }

            byte flags = ev.Flags;

            if ((flags & BypassFlags) > 0)
            {
                if (plugin.Config.VerboseMode)
                    Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) has bypass flags (flags: {(int)flags}). Skipping checks.");
                return;
            }

            if (plugin.Config.VerboseMode)
                Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) doesn't have bypass flags (flags: {(int)flags}).");

            if (plugin.Config.VpnCheck && plugin.VPN.BlacklistedIPCheck(ev.Request.RemoteEndPoint.Address, ev.UserId))
            {
                writer.Reset();
                writer.Put((byte)10);
                writer.Put(plugin.Config.VpnKickMessage); //Limit of 400 characters due to the limit of the UDP packet.
                ev.Request.Reject(writer);
                return;
            }

            _ = Check(ev); //Do checks on in task to prevent holding up the game.
        }

        public void Verified(VerifiedEventArgs ev)
        {
            PlayerToKick tk;
            if (!ToKick.TryGetValue(new PlayerToKick(ev.Player.UserId, KickReason.AccountAge), out tk) && !ToKick.TryGetValue(new PlayerToKick(ev.Player.UserId, KickReason.AccountPlaytime), out tk))
                return;

            ToKick.Remove(tk);

            switch (tk.reason)
            {
                case KickReason.AccountAge:
                    ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountAgeCheckKickMessage.Replace("%MINIMUMAGE%", plugin.Config.SteamMinAge.ToString()));
                    break;
                case KickReason.AccountPlaytime:
                    ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountPlaytimeCheckKickMessage.Replace("%MINIMUMPLAYTIME%", plugin.Config.SteamMinPlaytime.ToString()));
                    break;
                case KickReason.VPN:
                    ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.VpnKickMessage);
                    break;
            }
        }

        public void WaitingForPlayers()
        {
            Filesystem.CheckFileSystem();
            Filesystem.LoadData();
        }

        public void RoundEnded(RoundEndedEventArgs _)
        {
            ToKick.Clear();
            stopwatch.Reset();
            cleanupStopwatch.Reset();
            if (plugin.Config.VerboseMode)
                Log.Debug("Cleared ToKick HashSet.");
        }

        public async Task Check(PreAuthenticatingEventArgs ev)
        {
            //Account age check.
            if (plugin.Config.AccountAgeCheck)
            {
                if (!string.IsNullOrWhiteSpace(plugin.Config.SteamApiKey))
                {
                    if (await plugin.Account.CheckAccountAge(ev.Request.RemoteEndPoint.Address, ev.UserId))
                    {
                        Player player = Player.Get(ev.UserId);
                        if (player != null)
                            ServerConsole.Disconnect(player.Connection, plugin.Config.AccountAgeCheckKickMessage.Replace("%MINIMUMAGE", plugin.Config.SteamMinAge.ToString()));
                            
                        else
                            StartStopwatch();
                        ToKick.Add(new PlayerToKick(ev.UserId, KickReason.AccountAge));
                        return;
                    }
                }

                else
                    Log.Warn($"An account age check cannot be performed for {ev.UserId} ({ev.Request.RemoteEndPoint.Address}). Steam API key is null.");

            }

            //Account playtime check.
            if (plugin.Config.AccountPlaytimeCheck)
            {
                if (!string.IsNullOrWhiteSpace(plugin.Config.SteamApiKey))
                {
                    if (await plugin.Account.CheckAccountPlaytime(ev.Request.RemoteEndPoint.Address, ev.UserId))
                    {
                        Player player = Player.Get(ev.UserId);
                        if (player != null)
                            ServerConsole.Disconnect(player.Connection, plugin.Config.AccountPlaytimeCheckKickMessage.Replace("%MINIMUMPLAYTIME%", plugin.Config.SteamMinPlaytime.ToString()));
                        else
                            StartStopwatch();
                        ToKick.Add(new PlayerToKick(ev.UserId, KickReason.AccountPlaytime));
                    }
                }

                else
                    Log.Warn($"An account playtime check cannot be performed for {ev.UserId} ({ev.Request.RemoteEndPoint.Address}). Steam API key is null.");
            }


            //VPN Check.
            if (plugin.Config.VpnCheck)
            {
                if (!string.IsNullOrWhiteSpace(plugin.Config.IpHubApiKey))
                {
                    if (await plugin.VPN.CheckVPN(ev.Request.RemoteEndPoint.Address, ev.UserId))
                    {
                        Player player = Player.Get(ev.UserId);
                        if (player != null)
                            ServerConsole.Disconnect(player.Connection, plugin.Config.VpnKickMessage);
                        else
                            StartStopwatch();
                        ToKick.Add(new PlayerToKick(ev.UserId, KickReason.VPN));
                    }
                }

                else
                    Log.Warn($"A VPN check cannot be performed for {ev.Request.RemoteEndPoint.Address} ({ev.UserId}). IPHub API key is null.");

            }

            //Else, let them continue.
        }

        public void StartStopwatch()
        {
            if (stopwatch.IsRunning)
            {
                if (ToKick.Count <= 500 || cleanupStopwatch.ElapsedMilliseconds <= 240000)
                    return;

                //ToKick cleanup
                ToClear.Clear();
                uint secs = (uint)stopwatch.Elapsed.TotalSeconds - 180;

                foreach (PlayerToKick player in ToKick)
                {
                    if (player.creationTime < secs)
                        ToClear.Add(player);
                }

                foreach (PlayerToKick player in ToClear)
                    ToKick.Remove(player);

                ToClear.Clear();
                cleanupStopwatch.Restart();

                return;
            }

            stopwatch.Start();
            cleanupStopwatch.Start();
            if (plugin.Config.VerboseMode)
                Log.Debug("Stopwatch started.");
        }
    }
}
