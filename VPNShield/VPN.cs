﻿using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using EXILED;

namespace VPNShield
{
    public static class VPN
    {
        public static async Task<bool> CheckVPN(string ipAddress, string userID) //A result of TRUE will kick.
        {
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
                        Log.Error("VPN check could not complete. You have reached your API key's limit.");
                    }

                    else
                    {
                        Log.Error("VPN API connection error: " + webRequest.StatusCode + " - " + webRequest.Content.ReadAsStringAsync());
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
                        Log.Info(ipAddress + " (" + userID + ") is not a detectable VPN.");
                    }
                    //Add to whitelist here!!
                    WhitelistAdd(ipAddress);
                    return false;
                }

                else if (block == 1)
                {
                    if (Plugin.verboseMode)
                    {
                        Log.Info(ipAddress + " (" + userID + ") is a detectable VPN. Kicking..");
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
            using (StreamWriter whitelist = File.AppendText(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt")) 
            {
                whitelist.WriteLine(ipAddress);
            }
        }


        public static void BlackListAdd(string ipAddress)
        {
            Plugin.vpnBlacklistedIPs.Add(ipAddress);
            using (StreamWriter blacklist = File.AppendText(Plugin.exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt")) 
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
                    Log.Info(ipAddress + " (" + userID + ") has already passed a VPN check / is whitelisted.");
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
                    Log.Info(ipAddress + " (" + userID + ") is already known as a VPN / is blacklisted. Kicking.");
                }
                return true;
            }
            return false;
        }
    }
}
