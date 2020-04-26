using System.Collections.Generic;
using System.IO;
using EXILED;

namespace VPNShield
{
    public static class Setup
    {
        public static void CheckFileSystem()
        {
            if (!Directory.Exists(Plugin.exiledPath + "/VPNShield"))
            {
                Log.Warn(Plugin.exiledPath + "/VPNShield directory does not exist. Creating.");
                Directory.CreateDirectory(Plugin.exiledPath + "/VPNShield");
            }

            if (!File.Exists(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt"))
            {
                Log.Warn(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt does not exist. Creating.");
                File.WriteAllText(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt", null);
            }

            if (!File.Exists(Plugin.exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt"))
            {
                Log.Warn(Plugin.exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt does not exist. Creating.");
                File.WriteAllText(Plugin.exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt", null);
            }

            if (!File.Exists(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt"))
            {
                Log.Warn(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt does not exist. Creating.");
                File.WriteAllText(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt", null);
            }

            if (!File.Exists(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
            {
                Log.Warn(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt does not exist. Creating.");
                File.WriteAllText(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt", null);
            }

            Log.Info("File system check complete.");
        }

        public static void ReloadConfig()
        {
            Plugin.accountCheck = Plugin.Config.GetBool("vs_accountcheck", false);
            Plugin.steamAPIKey = Plugin.Config.GetString("vs_steamapikey", null);
            Plugin.minimumAccountAge = Plugin.Config.GetInt("vs_accountminage", 14);
            Plugin.accountCheckKickMessage = Plugin.Config.GetString("vs_accountkickmessage", "Your account must be at least " + Plugin.minimumAccountAge.ToString() + " day(s) old to play on this server.");

            Plugin.vpnCheck = Plugin.Config.GetBool("vs_vpncheck", true);
            Plugin.ipHubAPIKey = Plugin.Config.GetString("vs_vpnapikey", null);
            Plugin.vpnKickMessage = Plugin.Config.GetString("vs_vpnkickmessage", "VPNs and proxies are forbidden on this server.");

            Plugin.verboseMode = Plugin.Config.GetBool("vs_verbose", false);
            Plugin.updateChecker = Plugin.Config.GetBool("vs_checkforupdates", true);

            if (Plugin.verboseMode) { Log.Info("Verbose mode is enabled."); }
            if (Plugin.accountCheck && Plugin.steamAPIKey == null) { Log.Info("This plugin requires a Steam API Key! Get one for free at https://steamcommunity.com/dev/apikey, and set it to vs_steamapikey!"); }
            if (Plugin.vpnCheck && Plugin.ipHubAPIKey == null) { Log.Info("This plugin requires a VPN API Key! Get one for free at https://iphub.info, and set it to vs_vpnapikey!"); }
            Log.Info("Configuration loaded.");
        }

        public static void LoadData()
        {
            Plugin.vpnWhitelistedIPs = null;
            Plugin.vpnBlacklistedIPs = null;
            Plugin.accountWhitelistedUserIDs = null;
            Plugin.checksWhitelistedUserIDs = null;

            Plugin.vpnWhitelistedIPs = new HashSet<string>(FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt")); //Known IPs that are not VPNs.
            Plugin.vpnBlacklistedIPs = new HashSet<string>(FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt")); //Known IPs that ARE VPNs.
            Plugin.accountWhitelistedUserIDs = new HashSet<string>(FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt")); //Known UserIDs that ARE old enough.
            Plugin.checksWhitelistedUserIDs = new HashSet<string>(FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt")); //UserIDs that can bypass VPN AND account checks.
            Log.Info("Data loaded.");
        }
    }
}

