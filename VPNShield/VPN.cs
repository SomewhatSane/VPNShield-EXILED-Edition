using System;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VPNShield
{
    public static class VPN
    {
        private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static string exiledPath = Path.Combine(appData, "Plugins");
        public static bool CheckVPN(string ipAddress, string userID)
        {

            if (Plugin.ipHubAPIKey == null)
            {
                return false; //Just add this incase someone has small brain and forgot to add an API Key.
            }

            //Check for Whitelisted UserID

            if (WhitelistedUsersCheck(ipAddress, userID)) //Check to see if they are a Whitelisted user that bypasses the VPN check.
            {
                return false;
            }

            if (BlacklistedIPCheck(ipAddress, userID)) //See if the IP Address is already blacklisted.
            {
                return true;
            }

            if (WhitelistedIPCheck(ipAddress, userID)) //See if the IP Address is already whitelisted.
            {
                return false;
            }



            HttpWebResponse response = null;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://v2.api.iphub.info/ip/" + ipAddress);
                webRequest.Headers.Add("x-key", Plugin.ipHubAPIKey);
                webRequest.Method = "GET";

                response = (HttpWebResponse)webRequest.GetResponse();

                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                JObject json = JObject.Parse(responseString);

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
                        Plugin.Info(ipAddress + " (" + userID + ") is a detectable VPN. Kicking.");
                    }

                    //Add to blacklist to prevent loads of calls!
                    BlackListAdd(ipAddress);
                    return true;
                }


            }

            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    response.Close();
                    response = (HttpWebResponse)e.Response;

                    if ((int)response.StatusCode == 429)
                    {
                        Plugin.Info("VPN check could not complete. You have reached your API key's limit.");
                    }
                    else
                    {
                        Plugin.Info("VPN connection error: " + response.StatusCode);
                    }

                }

                else
                {
                    Plugin.Info("VPN API connection error: " + e.Status.ToString());
                }
            }

            finally
            {
                response.Close();
            }
            return false;
        }
        
        public static void WhitelistAdd(string ipAddress)
        {
            using (StreamWriter whitelist = File.AppendText(exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt"))
            {
                whitelist.WriteLine(ipAddress + "\n");
            }
        }


        public static void BlackListAdd(string ipAddress)
        {
            using (StreamWriter blacklist = File.AppendText(exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt"))
            {
                blacklist.Write(ipAddress + "\n");
            }
        }

        public static bool WhitelistedIPCheck(string ipAddress, string userID)
        {
            string whitelistedIPsPath = exiledPath + "/VPNShield/VPNShield-WhitelistIPs.txt";
            using (StreamReader sr = File.OpenText(whitelistedIPsPath))
            {
                string[] whitelistedIPs = File.ReadAllLines(whitelistedIPsPath);
                for (int x = 0; x < whitelistedIPs.Length; x++)
                {
                    if (ipAddress == whitelistedIPs[x])
                    {
                        if (Plugin.verboseMode)
                        {
                            Plugin.Info(ipAddress + " (" + userID + ") has already passed a VPN check / is whitelisted.");
                        }

                        sr.Close();
                        return true;
                    }
                }

            }
            return false;
        }

        public static bool BlacklistedIPCheck(string ipAddress, string userID)
        {
            string blacklistedIPsPath = exiledPath + "/VPNShield/VPNShield-BlacklistIPs.txt";
            using (StreamReader sr = File.OpenText(blacklistedIPsPath))
            {
                string[] blacklistedIPs = File.ReadAllLines(blacklistedIPsPath);
                for (int x = 0; x < blacklistedIPs.Length; x++)
                {
                    if (ipAddress == blacklistedIPs[x])
                    {
                        if (Plugin.verboseMode)
                        {
                            Plugin.Info(ipAddress + " (" + userID + ") is already known as a VPN / is blacklisted. Kicking.");
                        }

                        sr.Close();
                        return true;
                    }
                }

            }

            return false;
        }

        public static bool WhitelistedUsersCheck(string ipAddress, string userID)
        {
            string whitelistedUserIDsPath = exiledPath + "/VPNShield/VPNShield-WhitelistUserIDs.txt";
            using (StreamReader sr = File.OpenText(whitelistedUserIDsPath))
            {
                string[] whitelistedUserIDs = File.ReadAllLines(whitelistedUserIDsPath);
                for (int x = 0; x < whitelistedUserIDs.Length; x++)
                {
                    if (userID == whitelistedUserIDs[x])
                    {
                        if (Plugin.verboseMode)
                        {
                            Plugin.Info("UserID " + userID + " (" + ipAddress + ") is whitelisted from VPN checks. Bypassing checks.");
                        }

                        sr.Close();
                        return true;
                    }
                }

            }
            return false;
        }
    }
}
