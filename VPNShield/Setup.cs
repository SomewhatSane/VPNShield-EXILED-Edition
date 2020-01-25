using System;
using System.IO;

namespace VPNShield
{

    public static class Setup
    {
        private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string exiledPath = Path.Combine(appData, "Plugins");

        public static void CheckFileSystem()
        {
            if (!Directory.Exists(exiledPath + "/VPNShield"))
            {
                Plugin.Debug(exiledPath + "/VPNShield directory does not exist. Creating.");
                Plugin.Info(exiledPath + "/VPNShield directory does not exist. Creating.");
                Directory.CreateDirectory(exiledPath + "/VPNShield");
            }

            if (!File.Exists(exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt"))
            {
                Plugin.Debug(exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt does not exist. Creating.");
                Plugin.Info(exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt does not exist. Creating.");
                File.WriteAllText(exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt", null);
            }

            if (!File.Exists(exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt"))
            {
                Plugin.Debug(exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt does not exist. Creating.");
                Plugin.Info(exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt does not exist. Creating.");
                File.WriteAllText(exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt", null);
            }

            if (!File.Exists(exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt"))
            {
                Plugin.Debug(exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt does not exist. Creating.");
                Plugin.Info(exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt does not exist. Creating.");
                File.WriteAllText(exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt", null);
            }
            Plugin.Info("File system setup check complete.");
        }
    }
}

