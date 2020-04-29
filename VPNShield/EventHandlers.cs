#define Pre10 //Remove this symbol to make the plugin compatible with game versions 10.0.0 and newer instead of 9.1.3
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EXILED;
using EXILED.Extensions;
using LiteNetLib.Utils;

namespace VPNShield
{
    public class EventHandlers
    {
        public EventHandlers(Plugin plugin)
        {
        }
        
        private const byte BypassFlags = (1 << 1) | (1 << 3); //IgnoreBans or IgnoreGeoblock
        
        internal static readonly Stopwatch stopwatch = new Stopwatch();
        private static readonly Stopwatch cleanupStopwatch = new Stopwatch();
        private static readonly HashSet<PlayerToKick> ToKick = new HashSet<PlayerToKick>();
        private static readonly HashSet<PlayerToKick> ToClear = new HashSet<PlayerToKick>();
        private static readonly NetDataReader reader = new NetDataReader();
        private static readonly NetDataWriter writer = new NetDataWriter();

        public void OnRACommand(ref RACommandEvent ev)
        {
            if (!ev.Command.Split(' ')[0].Equals("VS_RELOAD", StringComparison.InvariantCultureIgnoreCase)) return;
            ev.Allow = false;
            Setup.ReloadConfig();
            Setup.LoadData();
            ev.Sender.RAMessage("Reloaded VPNShield.");
            Log.Info("Reloaded VPNShield.");
        }

        public void OnPreAuth(ref PreauthEvent ev)
        {
            if (GlobalWhitelist.GlobalWhitelistCheck(ev.UserId))
            {
                if (Plugin.verboseMode)
                    Log.Info($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is whitelisted from VPN and account age checks. Skipping checks.");
                
                return;
            }
            
            if (ev.UserId.Contains("@northwood", StringComparison.InvariantCultureIgnoreCase))
            {
                if (Plugin.verboseMode)
                    Log.Info($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is a Northwood Studios member. Skipping checks.");
                return;
            }
            
#if Pre10
            reader.SetSource(ev.Request.Data.RawData, 15); //13 bytes of handshake + 2 bytes of data
            if (reader.TryGetString(out var s) && reader.TryGetULong(out var e) &&
                reader.TryGetByte(out var flags)) //We don't care about UserID and preauth expiration date
#else
            reader.SetSource(ev.Request.Data.RawData, 20); //13 bytes of handshake and 3 bytes and 1 int offset
            if (reader.TryGetBytesWithLength(out var b) && reader.TryGetString(out var s) &&
                reader.TryGetULong(out var e) && reader.TryGetByte(out var flags)) //We don't care about preauth challenge stuff, UserID and preauth expiration date
#endif

            {
                if ((flags & BypassFlags) > 0)
                {
                    if (Plugin.verboseMode)
                        Log.Info(
                            $"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) have bypass flags (flags: {(int)flags}). Skipping checks.");
                    return;
                }
                
                if (Plugin.verboseMode)
                    Log.Info(
                        $"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) doesn't have bypass flags (flags: {(int)flags}).");
            }
            else
                Log.Error($"Failed to process preauth token of user {ev.UserId} ({ev.Request.RemoteEndPoint.Address})! {BitConverter.ToString(ev.Request.Data.RawData)}");

            if (Plugin.vpnCheck && VPN.BlacklistedIPCheck(ev.Request.RemoteEndPoint.Address, ev.UserId))
            {
                ev.Allow = false;
                
                writer.Reset();
                writer.Put((byte)10); //Reason: Custom
                writer.Put(Plugin.vpnKickMessageShort); //Limit here is 400 characters due to UDP packet size limit
                ev.Request.Reject(writer);
                return;
            }

            _ = Check(ev); //Do checks on in task to prevent holding up the game.
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (!ToKick.TryGetValue(new PlayerToKick(ev.Player.characterClassManager.UserId, KickReason.Account),
                out var tk)) return;

            ToKick.Remove(tk);
            ServerConsole.Disconnect(ev.Player.characterClassManager.connectionToClient,
                tk.reason == KickReason.VPN ? Plugin.vpnKickMessage : Plugin.accountCheckKickMessage);
        }

        public void OnRoundEnd()
        {
            ToKick.Clear();
            stopwatch.Reset();
            cleanupStopwatch.Reset();
            
            if (Plugin.verboseMode)
                Log.Info($"Cleared ToKick HashSet.");
        }

        private static async Task Check(PreauthEvent ev)
        {
            //Account check.
            if (Plugin.accountCheck)
            {
                if (await Account.CheckAccount(ev.Request.RemoteEndPoint.Address, ev.UserId))
                {
                    ReferenceHub hub = Player.GetPlayer(ev.UserId);
                    if (hub != null)
                        ServerConsole.Disconnect(hub.characterClassManager.connectionToClient,
                            Plugin.accountCheckKickMessage);
                    else
                    {
                        StartStopwatch();
                        ToKick.Add(new PlayerToKick(ev.UserId, KickReason.Account));
                    }
                    return;
                }
            }

            //VPN Check.
            if (Plugin.vpnCheck)
            {
                if (await VPN.CheckVPN(ev.Request.RemoteEndPoint.Address, ev.UserId))
                {
                    ReferenceHub hub = Player.GetPlayer(ev.UserId);
                    if (hub != null)
                        ServerConsole.Disconnect(hub.characterClassManager.connectionToClient,
                            Plugin.vpnKickMessage);
                    else
                    {
                        StartStopwatch();
                        ToKick.Add(new PlayerToKick(ev.UserId, KickReason.VPN));
                    }

                    //return; //Currently this return is redundant
                }
            }

            //Else, let them continue.
        }
        
        public void OnWaitingForPlayers()
        {
            Setup.ReloadConfig();
            Setup.LoadData();
        }

        private static void StartStopwatch()
        {
            if (stopwatch.IsRunning)
            {
                if (ToKick.Count <= 500 || cleanupStopwatch.ElapsedMilliseconds <= 240000) return;
                
                //ToKick cleanup
                ToClear.Clear();
                uint secs = (uint)stopwatch.Elapsed.TotalSeconds - 180;
                
                foreach (PlayerToKick player in ToKick)
                    if (player.creationTime < secs)
                        ToClear.Add(player);

                foreach (PlayerToKick player in ToClear)
                    ToKick.Remove(player);
                
                ToClear.Clear();
                cleanupStopwatch.Restart();

                return;
            }
            
            stopwatch.Start();
            cleanupStopwatch.Start();
            if (Plugin.verboseMode)
                Log.Info($"Stopwatch started.");
        }
    }
}
