namespace VPNShield
{
    internal static class GlobalWhitelist
    {
        internal static bool GlobalWhitelistCheck(string userID) => Plugin.checksWhitelistedUserIDs.Contains(userID);
    }
}
