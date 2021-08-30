using Exiled.API.Features;
using System.IO;
using System.Net;

namespace VPNShield
{
    public static class Filesystem
    {
        public static void CheckFileSystem()
        {
            if (!Directory.Exists($"{Plugin.exiledPath}/VPNShield"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield directory does not exist. Creating.");
                Directory.CreateDirectory($"{Plugin.exiledPath}/VPNShield");
            }

            if (!File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt does not exist. Creating.");
                File.WriteAllText($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt", null);
            }

            if (!File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt does not exist. Creating.");
                File.WriteAllText($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt", null);
            }

            if (!File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt does not exist. Creating.");
                File.WriteAllText($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt", null);
            }

            if (!File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt does not exist. Creating.");
                File.WriteAllText($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt", null);
            }

            if (!File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt does not exist. Creating.");
                File.WriteAllText($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt", null);
            }

            Log.Info($"File system check complete. Working directory is: {Path.Combine(Plugin.exiledPath, "VPNShield")}.");
        }

        public static void LoadData()
        {
            Plugin.vpnWhitelistedIPs.Clear();
            Plugin.vpnBlacklistedIPs.Clear();
            Plugin.accountAgeWhitelistedUserIDs.Clear();
            Plugin.accountPlaytimeWhitelistedUserIDs.Clear();
            Plugin.checksWhitelistedUserIDs.Clear();

            foreach (string ip in FileManager.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt"))
            {
                if (IPAddress.TryParse(ip, out IPAddress addr))
                    Plugin.vpnWhitelistedIPs.Add(addr);
            }

            foreach (string ip in FileManager.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt"))
            {
                if (IPAddress.TryParse(ip, out IPAddress addr))
                    Plugin.vpnBlacklistedIPs.Add(addr);
            }

            foreach (string id in FileManager.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt"))
                Plugin.checksWhitelistedUserIDs.Add(id);
            foreach (string id in FileManager.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
                Plugin.accountAgeWhitelistedUserIDs.Add(id);
            foreach (string id in FileManager.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt"))
                Plugin.accountPlaytimeWhitelistedUserIDs.Add(id);

            Log.Info("Data loaded.");
        }
    }
}

