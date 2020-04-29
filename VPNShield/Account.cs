using System;
using System.Globalization;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using EXILED;

namespace VPNShield
{
    internal static class Account
    {
        internal static async Task<bool>CheckAccount(IPAddress ipAddress, string userID) //A result of TRUE will kick.
        {
            if (CheckWhitelist(ipAddress, userID))
                return false; //Check for all ready known accounts.

            if (!userID.Contains("@", StringComparison.InvariantCulture))
                return false; //Invalid UserID

            var split = userID.Split('@');
            switch (split[1].ToLower(CultureInfo.InvariantCulture))
            {
                case "steam":
                    using (HttpClient client = new HttpClient())
                    {
                        var webRequest = await client.GetAsync("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + Plugin.steamAPIKey + "&steamids=" + split[0]);
                        if (!webRequest.IsSuccessStatusCode)
                        {
                            Log.Error(webRequest.StatusCode == (HttpStatusCode) 429
                                ? "Steam account check could not complete. You have reached your API key's limit."
                                : $"Steam API connection error: {webRequest.StatusCode} - {webRequest.Content.ReadAsStringAsync()}");
                            return false;
                        }

                        string apiResponse = await webRequest.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(apiResponse);

                        int communityvisibilitystate = (int)json["response"]["players"][0]["communityvisibilitystate"];
                        if (Plugin.accountKickPrivate && communityvisibilitystate == 1)
                        {
                            if (Plugin.verboseMode)
                                Log.Info("UserID " + userID + " (" + ipAddress + ") cannot have their account age checked due to their privacy settings. Kicking..");
                            return true;
                        }

                        int timecreated = (int)json["response"]["players"][0]["timecreated"];
                        DateTime creationDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timecreated);

                        int accountAge = (int)(DateTime.Today - creationDateTime).TotalDays;

                        if (accountAge < Plugin.minimumAccountAge)
                        {
                            if (Plugin.verboseMode)
                                Log.Info("UserID " + userID + " (" + ipAddress + ") is too young to be on this server (Account is " + accountAge + " day(s) old). Kicking..");
                            return true;
                        }

                        if (Plugin.verboseMode)
                            Log.Info("UserID " + userID + " (" + ipAddress + ") is old enough to be on this server (Account is " + accountAge + " day(s) old).");
                        WhitelistAdd(userID);
                        return false;
                    }
                    
                case "discord":
                case "northwood":
                    return false; //Ignore them
                
                default:
                    return false; //Any other UserID
            }
        }

        private static bool CheckWhitelist(IPAddress ipAddress, string userID)
        {
            if (!Plugin.accountWhitelistedUserIDs.Contains(userID)) return false;
            if (Plugin.verboseMode)
                Log.Info("UserID " + userID + " (" + ipAddress + ") is already known to be old enough. Skipping account age check.");
            return true;
        }

        private static void WhitelistAdd(string userID)
        {
            Plugin.accountWhitelistedUserIDs.Add(userID);
            using (StreamWriter whitelist = File.AppendText(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
                whitelist.WriteLine(userID);
        }
    }
}
