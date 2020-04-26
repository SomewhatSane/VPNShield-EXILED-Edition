using EXILED;

namespace VPNShield
{
    public static class GlobalWhitelist
    {
        public static bool GlobalWhitelistCheck(string ipAddress, string userID)
        {
            if (Plugin.checksWhitelistedUserIDs.Contains(userID))
            {
                if (Plugin.verboseMode)
                {
                    Log.Info("UserID " + userID + " (" + ipAddress + ") is whitelisted from VPN and account age checks. Skipping checks.");
                }
                return true;
            }

            return false;
        }
    }
}
