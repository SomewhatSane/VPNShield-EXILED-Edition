using System.IO;
using System.Net;
using EXILED;

namespace VPNShield
{
    internal static class Setup
    {
        internal static void CheckFileSystem()
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

        internal static void ReloadConfig()
        {
            Plugin.accountCheck = EXILED.Plugin.Config.GetBool("vs_accountcheck", false);
            Plugin.accountKickPrivate = EXILED.Plugin.Config.GetBool("vs_accountkickprivate", true);
            Plugin.steamAPIKey = EXILED.Plugin.Config.GetString("vs_steamapikey", null);
            Plugin.minimumAccountAge = EXILED.Plugin.Config.GetInt("vs_accountminage", 14);
            Plugin.accountCheckKickMessage = EXILED.Plugin.Config.GetString("vs_accountkickmessage", "Your account must be at least " + Plugin.minimumAccountAge.ToString() + " day(s) old to play on this server or your account age could not be checked due to privacy settings.");

            Plugin.vpnCheck = EXILED.Plugin.Config.GetBool("vs_vpncheck", true);
            Plugin.ipHubAPIKey = EXILED.Plugin.Config.GetString("vs_vpnapikey", null);
            Plugin.vpnKickMessage = EXILED.Plugin.Config.GetString("vs_vpnkickmessage", "VPNs and proxies are forbidden on this server.");
            Plugin.vpnKickMessageShort = Plugin.vpnKickMessage.Length > 400
                ? Plugin.vpnKickMessage.Substring(0, 400)
                : Plugin.vpnKickMessage;

            Plugin.verboseMode = EXILED.Plugin.Config.GetBool("vs_verbose", false);
            Plugin.updateChecker = EXILED.Plugin.Config.GetBool("vs_checkforupdates", true);

            if (Plugin.verboseMode)
                Log.Info("Verbose mode is enabled.");
            
            if (Plugin.accountCheck && string.IsNullOrEmpty(Plugin.steamAPIKey))
            {
                Plugin.accountCheck = false;
                Log.Info("This plugin requires a Steam API Key! Get one for free at https://steamcommunity.com/dev/apikey, and set it to vs_steamapikey!");
            }

            if (Plugin.vpnCheck && string.IsNullOrEmpty(Plugin.ipHubAPIKey))
            {
                Plugin.vpnCheck = false;
                Log.Info("This plugin requires a VPN API Key! Get one for free at https://iphub.info, and set it to vs_vpnapikey!");
            }
            
            Log.Info("Configuration loaded.");
        }

        internal static void LoadData()
        {
            Plugin.vpnWhitelistedIPs.Clear();
            Plugin.vpnBlacklistedIPs.Clear();
            Plugin.accountWhitelistedUserIDs.Clear();
            Plugin.checksWhitelistedUserIDs.Clear();

            foreach (var ip in FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt"))
                if (IPAddress.TryParse(ip, out var addr))
                    Plugin.vpnWhitelistedIPs.Add(addr);
            
            foreach (var ip in FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt"))
                if (IPAddress.TryParse(ip, out var addr))
                    Plugin.vpnBlacklistedIPs.Add(addr);

            foreach (var id in FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
                Plugin.accountWhitelistedUserIDs.Add(id);
            
            foreach (var id in FileManager.ReadAllLines(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt"))
                Plugin.checksWhitelistedUserIDs.Add(id);
            
            Log.Info("Data loaded.");
        }
    }
}

