using Exiled.API.Features;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace VPNShield
{
    public class Account
    {
        private readonly Plugin plugin;
        public Account(Plugin plugin) => this.plugin = plugin;

        public async Task<bool> CheckAccount(IPAddress ipAddress, string userID) //A result of TRUE will kick.
        {
            if (CheckWhitelist(ipAddress, userID))
                return false; //Check for known accounts.

            if (!userID.Contains("@", StringComparison.InvariantCulture))
                return false; //Invalid UserID

            string[] split = userID.Split('@');
            switch (split[1].ToLower(CultureInfo.InvariantCulture))
            {
                case "steam":
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage webRequest = await client.GetAsync("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + plugin.Config.SteamApiKey + "&steamids=" + split[0]);
                        if (!webRequest.IsSuccessStatusCode)
                        {
                            Log.Error(webRequest.StatusCode == (HttpStatusCode)429
                                ? "Steam account check could not complete. You have reached your API key's limit."
                                : $"Steam API connection error: {webRequest.StatusCode} - {webRequest.Content.ReadAsStringAsync()}");
                            return false;
                        }

                        string apiResponse = await webRequest.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(apiResponse);

                        int communityvisibilitystate = (int)json["response"]["players"][0]["communityvisibilitystate"];
                        if (plugin.Config.AccountKickPrivate && communityvisibilitystate == 1)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Info($"UserID {userID} ({ipAddress}) cannot have their account age checked due to their privacy settings. Kicking..");

                            return true;
                        }

                        int timecreated = (int)json["response"]["players"][0]["timecreated"];
                        DateTime creationDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timecreated);

                        int accountAge = (int)(DateTime.Today - creationDateTime).TotalDays;

                        if (accountAge < plugin.Config.SteamMinAge)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Info($"UserID {userID} ({ipAddress}) is too young to be on this server (account is {accountAge} day(s) old). Kicking..");

                            return true;
                        }

                        if (plugin.Config.VerboseMode)
                            Log.Info($"UserID {userID} ({ipAddress}) is old enough to be on this server (account is {accountAge} day(s) old).");

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

        private bool CheckWhitelist(IPAddress ipAddress, string userID)
        {
            if (!Plugin.accountWhitelistedUserIDs.Contains(userID))
                return false;

            if (plugin.Config.VerboseMode)
                Log.Info($"UserID {userID} ({ipAddress}) is already known to be old enough. Skipping account age check.");

            return true;
        }

        private void WhitelistAdd(string userID)
        {
            Plugin.accountWhitelistedUserIDs.Add(userID);
            using (StreamWriter whitelist = File.AppendText(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
                whitelist.WriteLine(userID);
        }
    }
}
