using EXILED;

namespace VPNShield
{
    public static class GlobalWhitelist
    {
        public static bool GlobalWhitelistCheck(string userID) => Plugin.checksWhitelistedUserIDs.Contains(userID);
    }
}
