using System;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace VPNShield
{
    public static class VPN
    {
        private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string exiledPath = Path.Combine(appData, "Plugins");

        public static async Task<bool> CheckVPN(string ipAddress, string userID)
        {
            if (Plugin.ipHubAPIKey == null) { return false; } //Just add this incase someone has small brain and forgot to add an API Key.
            if (GlobalWhitelist.GlobalWhitelistCheck(ipAddress, userID)) { return false; } //User's allowed to bypass both VPN and Account checks.
            if (BlacklistedIPCheck(ipAddress, userID)) { return true; } //Known VPN IPs.
            if (WhitelistedIPCheck(ipAddress, userID)) { return false; } //Known good IPs. Else..

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-key", Plugin.ipHubAPIKey);
                var webRequest = await client.GetAsync("https://v2.api.iphub.info/ip/" + ipAddress);

                if (!webRequest.IsSuccessStatusCode)
                {
                    if (webRequest.StatusCode == (HttpStatusCode)429)
                    {
                        Plugin.Info("VPN check could not complete. You have reached your API key's limit.");
                    }

                    else
                    {
                        Plugin.Info("VPN API connection error: " + webRequest.StatusCode + " - " + webRequest.Content.ReadAsStringAsync());
                    }
                    return false;
                }

                string apiResponse = await webRequest.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(apiResponse);
                int block = json.Value<int>("block");

                if (block == 0 || block == 2) //2 is for unknown calls from the API. Strange.
                {
                    if (Plugin.verboseMode)
                    {
                        Plugin.Info(ipAddress + " (" + userID + ") is not a detectable VPN.");
                    }
                    //Add to whitelist here!!
                    WhitelistAdd(ipAddress);
                    return false;
                }

                else if (block == 1)
                {
                    if (Plugin.verboseMode)
                    {
                        Plugin.Info(ipAddress + " (" + userID + ") is a detectable VPN. Kicking..");
                    }

                    //Add to blacklist to prevent loads of calls!
                    BlackListAdd(ipAddress);
                    return true;
                }
            }
            

            
            return false;
        }
        
        public static void WhitelistAdd(string ipAddress)
        {
            Plugin.vpnWhitelistedIPs.Add(ipAddress);
            using (StreamWriter whitelist = File.AppendText(exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt")) 
            {
                whitelist.WriteLine(ipAddress);
            }
        }


        public static void BlackListAdd(string ipAddress)
        {
            Plugin.vpnBlacklistedIPs.Add(ipAddress);
            using (StreamWriter blacklist = File.AppendText(exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt")) 
            {
                blacklist.WriteLine(ipAddress);
            }
        }

        public static bool WhitelistedIPCheck(string ipAddress, string userID)
        {
            if (Plugin.vpnWhitelistedIPs.Contains(ipAddress))
            {
                if (Plugin.verboseMode)
                {
                    Plugin.Info(ipAddress + " (" + userID + ") has already passed a VPN check / is whitelisted.");
                }
                return true;
            }
            return false;
        }

        public static bool BlacklistedIPCheck(string ipAddress, string userID)
        {
            if (Plugin.vpnBlacklistedIPs.Contains(ipAddress))
            {
                if (Plugin.verboseMode)
                {
                    Plugin.Info(ipAddress + " (" + userID + ") is already known as a VPN / is blacklisted. Kicking.");
                }
                return true;
            }
            return false;
        }
    }
}
