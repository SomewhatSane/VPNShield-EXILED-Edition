using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EXILED;

namespace VPNShield
{
    public class GlobalWhitelist
    {
        private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string exiledPath = Path.Combine(appData, "Plugins");
        public static bool GlobalWhitelistCheck(string ipAddress, string userID)
        {
            string GlobalWL = exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt";
            using (StreamReader sr = File.OpenText(GlobalWL))
            {
                string[] whitelistedUsers = File.ReadAllLines(GlobalWL);
                for (int x = 0; x < whitelistedUsers.Length; x++)
                {
                    if (userID == whitelistedUsers[x])
                    {
                        if (Plugin.verboseMode)
                        {
                            Plugin.Info("UserID " + userID + " (" + ipAddress + ") whitelisted from VPN and account age checks. Skipping checks.");
                        }

                        sr.Close();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
