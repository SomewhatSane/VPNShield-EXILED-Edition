using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace VPNShield
{
    public class Account
    {
        private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string exiledPath = Path.Combine(appData, "Plugins");
        public static async Task<bool> CheckAccount(string ipAddress, string userID)
        {
            if (GlobalWhitelist.GlobalWhitelistCheck(ipAddress, userID)) { return false; } //Check for globally whitelisted accounts.
            if (CheckWhitelist(ipAddress, userID)) { return false; } //Check for all ready known accounts.

            if (userID.EndsWith("@steam"))
            {
                using (HttpClient client = new HttpClient())
                {
                    var webRequest = await client.GetAsync("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + Plugin.steamAPIKey + "&steamids=" + userID.Replace("@steam", string.Empty));
                    if (!webRequest.IsSuccessStatusCode)
                    {
                        if (webRequest.StatusCode == (HttpStatusCode)429)
                        {
                            Plugin.Info("Steam account check could not complete. You have reached your API key's limit.");
                        }

                        else
                        {
                            Plugin.Info("Steam API connection error: " + webRequest.StatusCode + " - " + webRequest.Content.ReadAsStringAsync());
                        }
                        return false;
                    }

                    string apiResponse = await webRequest.Content.ReadAsStringAsync();

                    JObject json = JObject.Parse(apiResponse);
                    int timecreated = (int)json["response"]["players"][0]["timecreated"];

                    DateTime creationDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    creationDateTime = creationDateTime.AddSeconds(timecreated);

                    int accountAge = (int)(DateTime.Today - creationDateTime).TotalDays;

                    if (accountAge < Plugin.minimumAccountAge)
                    {
                        if (Plugin.verboseMode)
                        {
                            Plugin.Info("UserID " + userID + " (" + ipAddress + ") is too young to be on this server. (Account is " + accountAge + " day(s) old). Kicking..");
                        }
                        return true;
                    }

                    else
                    {
                        if (Plugin.verboseMode)
                        {
                            Plugin.Info("UserID " + userID + " (" + ipAddress + ") is old enough to be on this server. (Account is " + accountAge + " day(s) old).");
                        }
                        WhitelistAdd(userID);
                        return false;
                    }
                }

            }

            else if (userID.EndsWith("@discord"))
            {
                return false; //Add this later!!
            }

            else
            {   //Other domain or no domain.
                return false;
            }
        }

        public static bool CheckWhitelist(string ipAddress, string userID)
        {
            if (Plugin.accountWhitelistedUserIDs.Contains(userID))
            {
                if (Plugin.verboseMode)
                {
                    Plugin.Info("UserID " + userID + " (" + ipAddress + ") is already known to be old enough. Skipping account age check.");
                }

                return true;
            }
            return false;
        }

        public static void WhitelistAdd(string userID)
        {
            Plugin.accountWhitelistedUserIDs.Add(userID);
            using (StreamWriter whitelist = File.AppendText(exiledPath + "/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
            {
                whitelist.WriteLine(userID);
            }
        }
    }
}
