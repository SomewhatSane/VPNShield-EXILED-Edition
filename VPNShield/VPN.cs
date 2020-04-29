using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using EXILED;

namespace VPNShield
{
    public static class VPN
    {
        public static async Task<bool> CheckVPN(IPAddress ipAddress, string userID) //A result of TRUE will kick.
        {
            if (BlacklistedIPCheck(ipAddress, userID)) return true; //Known VPN IPs.
            if (WhitelistedIPCheck(ipAddress, userID)) return false; //Known good IPs. Else..

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-key", Plugin.ipHubAPIKey);
                var webRequest = await client.GetAsync("https://v2.api.iphub.info/ip/" + ipAddress);

                if (!webRequest.IsSuccessStatusCode)
                {
                    Log.Error(webRequest.StatusCode == (HttpStatusCode) 429
                        ? "VPN check could not complete. You have reached your API key's limit."
                        : $"VPN API connection error: {webRequest.StatusCode} - {webRequest.Content.ReadAsStringAsync()}");
                    return false;
                }

                string apiResponse = await webRequest.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(apiResponse);
                int block = json.Value<int>("block");

                switch (block)
                {
                    case 0:
                    case 2: //2 is for unknown calls from the API. Strange.
                    {
                        if (Plugin.verboseMode)
                            Log.Info(ipAddress + " (" + userID + ") is not a detectable VPN.");
                        
                        //Add to whitelist here!!
                        WhitelistAdd(ipAddress);
                        return false;
                    }
                    
                    case 1:
                    {
                        if (Plugin.verboseMode)
                            Log.Info(ipAddress + " (" + userID + ") is a detectable VPN. Kicking..");

                        //Add to blacklist to prevent loads of calls!
                        BlackListAdd(ipAddress);
                        return true;
                    }
                }
            }
            
            return false;
        }

        private static void WhitelistAdd(IPAddress ipAddress)
        {
            Plugin.vpnWhitelistedIPs.Add(ipAddress);
            using (StreamWriter whitelist = File.AppendText(Plugin.exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt")) 
                whitelist.WriteLine(ipAddress);
        }
        
        private static void BlackListAdd(IPAddress ipAddress)
        {
            Plugin.vpnBlacklistedIPs.Add(ipAddress);
            using (StreamWriter blacklist = File.AppendText(Plugin.exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt")) 
                blacklist.WriteLine(ipAddress);
        }

        private static bool WhitelistedIPCheck(IPAddress ipAddress, string userID)
        {
            if (!Plugin.vpnWhitelistedIPs.Contains(ipAddress)) return false;
            if (Plugin.verboseMode)
                Log.Info(ipAddress + " (" + userID + ") has already passed a VPN check / is whitelisted.");
            return true;
        }

        internal static bool BlacklistedIPCheck(IPAddress ipAddress, string userID)
        {
            if (!Plugin.vpnBlacklistedIPs.Contains(ipAddress)) return false;
            if (Plugin.verboseMode)
                Log.Info(ipAddress + " (" + userID + ") is already known as a VPN / is blacklisted. Kicking.");
            return true;
        }
    }
}
