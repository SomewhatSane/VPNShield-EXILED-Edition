using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EXILED;
using EXILED.Extensions;
using LiteNetLib.Utils;
using Telepathy;

namespace VPNShield
{
    public class EventHandlers
    {
        public Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;
        private const byte BypassFlags = (1 << 1) | (1 << 3); //IgnoreBans or IgnoreGeoblock
        
        internal static readonly Stopwatch stopwatch = new Stopwatch();
        internal static readonly Stopwatch cleanupStopwatch = new Stopwatch();
        private static readonly HashSet<PlayerToKick> ToKick = new HashSet<PlayerToKick>();
        private static readonly HashSet<PlayerToKick> ToClear = new HashSet<PlayerToKick>();
        private static readonly NetDataReader reader = new NetDataReader();
        private static readonly NetDataWriter writer = new NetDataWriter();

        public void OnRACommand(ref RACommandEvent ev)
        {
            if (ev.Command.ToUpper().Split(' ')[0] != "VS_RELOAD") return;
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
            
            if (ev.UserId.Contains("@northwood"))
            {
                if (Plugin.verboseMode)
                    Log.Info($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is a northwood studio member. Skipping checks.");
                return;
            }
            
            reader.SetSource(ev.Request.Data.RawData, 7); //3 bytes and 1 int offset

            if (reader.TryGetBytesWithLength(out var b) && reader.TryGetString(out var s) &&
                reader.TryGetULong(out var e) && reader.TryGetByte(out var flags)) //We don't care about preauth challenge stuff, UserID and preauth expiration date
            {
                if ((flags & BypassFlags) > 0)
                {
                    if (Plugin.verboseMode)
                        Log.Info(
                            $"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) has bypass flags. Skipping checks.");
                    return;
                }
            }
            else
                Log.Error($"Failed to process preuath token of user {ev.UserId} ({ev.Request.RemoteEndPoint.Address})!");

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
                    return;
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
