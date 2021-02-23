using Exiled.API.Features;
using Exiled.Events.EventArgs;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NorthwoodLib;

namespace VPNShield
{
    public class EventHandlers
    {
        private readonly Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        private const byte BypassFlags = (1 << 1) | (1 << 3); //IgnoreBans or IgnoreGeoblock

        internal static readonly Stopwatch stopwatch = new Stopwatch();
        private static readonly Stopwatch cleanupStopwatch = new Stopwatch();
        private static readonly HashSet<PlayerToKick> ToKick = new HashSet<PlayerToKick>();
        private static readonly HashSet<PlayerToKick> ToClear = new HashSet<PlayerToKick>();
        private static readonly NetDataWriter writer = new NetDataWriter();

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
            if (!ToKick.TryGetValue(new PlayerToKick(ev.Player.UserId, KickReason.Account),
                out PlayerToKick tk))
                return;

            ToKick.Remove(tk);
            ServerConsole.Disconnect(ev.Player.Connection,
                tk.reason == KickReason.VPN ? plugin.Config.VpnKickMessage : plugin.Config.AccountCheckKickMessage);
        }

        public void WaitingForPlayers()
        {
            Filesystem.CheckFileSystem();
            Filesystem.LoadData();
        }

        public void RoundEnded(RoundEndedEventArgs ev)
        {
            ToKick.Clear();
            stopwatch.Reset();
            cleanupStopwatch.Reset();
            if (plugin.Config.VerboseMode)
                Log.Debug($"Cleared ToKick HashSet.");
        }

        public async Task Check(PreAuthenticatingEventArgs ev)
        {
            //Account check.
            if (plugin.Config.AccountCheck)
            {
                if (await plugin.Account.CheckAccount(ev.Request.RemoteEndPoint.Address, ev.UserId))
                {
                    Player player = Player.Get(ev.UserId);
                    if (player != null)
                        ServerConsole.Disconnect(player.Connection,
                            plugin.Config.AccountCheckKickMessage);
                    else
                        StartStopwatch();
                    ToKick.Add(new PlayerToKick(ev.UserId, KickReason.Account));
                    return;
                }
            }

            //VPN Check.
            if (plugin.Config.VpnCheck)
            {
                if (await plugin.VPN.CheckVPN(ev.Request.RemoteEndPoint.Address, ev.UserId))
                {
                    Player player = Player.Get(ev.UserId);
                    if (player != null)
                    {
                        ServerConsole.Disconnect(player.Connection,
                            plugin.Config.VpnKickMessage);
                    }
                    else
                        StartStopwatch();
                    ToKick.Add(new PlayerToKick(ev.UserId, KickReason.VPN));
                }
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
                Log.Debug($"Stopwatch started.");
        }
    }
}
