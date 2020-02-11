using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using EXILED.Patches;
using LiteNetLib;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Mirror;
using LiteNetLib4Mirror;
using LiteNetLib.Utils;

namespace VPNShield
{
    public class EventHandlers
    {
        public Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;
        private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string exiledPath = Path.Combine(appData, "Plugins");


        public void OnRACommand(ref EXILED.RACommandEvent ev)
        {
            if (ev.Command.ToUpper() == "VS_RELOAD")
            {
                ev.Allow = false;
                ev.Sender.RaReply("Reloading VPNShield.", true, true, null);
                Plugin.accountCheck = Plugin.Config.GetBool("vs_accountcheck", false);
                Plugin.steamAPIKey = Plugin.Config.GetString("vs_steamapikey", null);
                Plugin.minimumAccountAge = Plugin.Config.GetInt("vs_accountminage", 14);
                Plugin.accountCheckKickMessage = Plugin.Config.GetString("vs_accountkickmessage", "Your account must be at least " + Plugin.minimumAccountAge.ToString() + " day(s) old to play on this server.");

                Plugin.vpnCheck = Plugin.Config.GetBool("vs_vpncheck", true);
                Plugin.ipHubAPIKey = Plugin.Config.GetString("vs_vpnapikey", null);
                Plugin.vpnKickMessage = Plugin.Config.GetString("vs_vpnkickmessage", "VPNs and proxies are forbidden on this server.");

                Plugin.verboseMode = Plugin.Config.GetBool("vs_verbose", false);
                Plugin.updateChecker = Plugin.Config.GetBool("vs_checkforupdates", true);

                Plugin.vpnWhitelistedIPs = null;
                Plugin.vpnBlacklistedIPs = null;
                Plugin.accountWhitelistedUserIDs = null;
                Plugin.checksWhitelistedUserIDs = null;

                Plugin.vpnWhitelistedIPs = new HashSet<string>(FileManager.ReadAllLines(exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt")); //Known IPs that are not VPNs.
                Plugin.vpnBlacklistedIPs = new HashSet<string>(FileManager.ReadAllLines(exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt")); //Known IPs that ARE VPNs.
                Plugin.accountWhitelistedUserIDs = new HashSet<string>(FileManager.ReadAllLines(exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt")); //Known UserIDs that ARE old enough.
                Plugin.checksWhitelistedUserIDs = new HashSet<string>(FileManager.ReadAllLines(exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt")); //UserIDs that can bypass VPN AND account checks.

                ev.Sender.RaReply("Reloaded VPNShield Config.", true, true, null);
            }
        }

        public void OnPlayerJoin(EXILED.PlayerJoinEvent ev)
        {
            _ = Check(ev);
        }

        public async Task Check(EXILED.PlayerJoinEvent ev)
        {
            //Account check.
            if (Plugin.accountCheck && Plugin.steamAPIKey != null)
            {
                if (await Account.CheckAccount(ev.Player.characterClassManager.connectionToClient.address, ev.Player.characterClassManager.UserId))
                {
                    ServerConsole.Disconnect(ev.Player.characterClassManager.connectionToClient, Plugin.accountCheckKickMessage);
                    return;
                }
            }

            //VPN Check.
            if (Plugin.vpnCheck && Plugin.ipHubAPIKey != null)
            {
                if (await VPN.CheckVPN(ev.Player.characterClassManager.connectionToClient.address, ev.Player.characterClassManager.UserId))
                {
                    ServerConsole.Disconnect(ev.Player.characterClassManager.connectionToClient, Plugin.vpnKickMessage);
                    return;
                }
            }

            //Else, let them continue.
        }
    }
}
