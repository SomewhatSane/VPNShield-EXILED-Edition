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
            //I'd love to make this more efficient but I'm not sure how to considering we have PlayerToKick objects and not nice dictionaries or anything.
            foreach (PlayerToKick tk in ToKick)
            {
                if (tk.userId == ev.Player.UserId)
                {
                    ToKick.Remove(tk);
                    switch (tk.reason)
                    {
                        case KickReason.None:
                            break;
                        case KickReason.AccountAge:
                            ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountAgeCheckKickMessage.Replace("%MINIMUMAGE%", plugin.Config.SteamMinAge.ToString()));
                            break;
                        case KickReason.AccountPlaytime:
                            ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountPlaytimeCheckKickMessage.Replace("%MINIMUMPLAYTIME%", plugin.Config.SteamMinPlaytime.ToString()));
                            break;
                        case KickReason.AccountPrivate:
                            ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountPrivateKickMessage);
                            break;
                        case KickReason.AccountSteamError:
                            ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountSteamErrorKickMessage);
                            break;
                        case KickReason.VPN:
                            ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.VpnKickMessage);
                            break;
                    }
                    return;
                }
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
                    //This should work but I'll have to make sure I test this in the morning.
                    string kickMessage = null;
                    KickReason kickReason = KickReason.None;
                    switch (await plugin.Account.CheckAccountAge(ev.Request.RemoteEndPoint.Address, ev.UserId))
                    {
                        case Account.AccountCheckResult.Fail:
                            kickMessage = plugin.Config.AccountAgeCheckKickMessage.Replace("%MINIMUMAGE", plugin.Config.SteamMinAge.ToString());
                            kickReason = KickReason.AccountAge;
                            break;
                        case Account.AccountCheckResult.Private:
                            kickMessage = plugin.Config.AccountPrivateKickMessage;
                            kickReason = KickReason.AccountPrivate;
                            break;
                        case Account.AccountCheckResult.APIError:
                            kickMessage = plugin.Config.AccountSteamErrorKickMessage;
                            kickReason = KickReason.AccountSteamError;
                            break;
                    }

                    if (kickReason != KickReason.None)
                    {
                        Player player = Player.Get(ev.UserId);
                        if (player != null)
                            ServerConsole.Disconnect(player.Connection, kickMessage);
                        else
                            StartStopwatch();
                        ToKick.Add(new PlayerToKick(ev.UserId, kickReason));
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
                    string kickMessage = null;
                    KickReason kickReason = KickReason.None;

                    switch (await plugin.Account.CheckAccountPlaytime(ev.Request.RemoteEndPoint.Address, ev.UserId))
                    {
                        //Same here, test this!
                        case Account.AccountCheckResult.Fail:
                            kickMessage = plugin.Config.AccountPlaytimeCheckKickMessage.Replace("%MINIMUMPLAYTIME%", plugin.Config.SteamMinPlaytime.ToString());
                            kickReason = KickReason.AccountPlaytime;
                            break;
                        case Account.AccountCheckResult.Private:
                            kickMessage = plugin.Config.AccountPrivateKickMessage;
                            kickReason = KickReason.AccountPrivate;
                            break;
                        case Account.AccountCheckResult.APIError:
                            kickMessage = plugin.Config.AccountSteamErrorKickMessage;
                            kickReason = KickReason.AccountSteamError;
                            break;
                    }

                    if (kickReason != KickReason.None)
                    {
                        Player player = Player.Get(ev.UserId);
                        if (player != null)
                            ServerConsole.Disconnect(player.Connection, kickMessage);
                        else
                            StartStopwatch();
                        ToKick.Add(new PlayerToKick(ev.UserId, kickReason));
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