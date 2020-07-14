namespace VPNShield
{
    internal static class GlobalWhitelist
    {
        internal static bool GlobalWhitelistCheck(string userID)
        {
            return Plugin.checksWhitelistedUserIDs.Contains(userID);
        }
    }
}
