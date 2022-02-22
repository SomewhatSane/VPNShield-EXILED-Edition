using Exiled.API.Features;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VPNShield.Objects;
using Utf8Json;

namespace VPNShield
{
    public class VPN
    {
        private readonly Plugin plugin;
        public VPN(Plugin plugin) => this.plugin = plugin;
        private readonly HttpClient client = new();

        public async Task<bool> CheckVPN(IPAddress ipAddress, string userID) //A result of TRUE will kick.
        {
            if (BlacklistedIPCheck(ipAddress, userID))
                return true; //Known VPN IPs.

            if (WhitelistedIPCheck(ipAddress, userID))
                return false; //Known good IPs. Else..

            if (!client.DefaultRequestHeaders.Contains("x-key"))   
                client.DefaultRequestHeaders.Add("x-key", plugin.Config.IpHubApiKey);

            HttpResponseMessage webRequest = await client.GetAsync($"https://v2.api.iphub.info/ip/{ipAddress}");

            if (!webRequest.IsSuccessStatusCode)
            {
                string errorResponse = await webRequest.Content.ReadAsStringAsync();
                Log.Error(webRequest.StatusCode == (HttpStatusCode)429
                    ? "VPN check could not complete. You have reached your API key's limit."
                    : $"VPN API connection error: {webRequest.StatusCode} - {errorResponse}");
                return false;
            }

            string apiResponse = await webRequest.Content.ReadAsStringAsync();
            IpHubApiResponse ipHubApiResponse = JsonSerializer.Deserialize<IpHubApiResponse>(apiResponse);

            switch (ipHubApiResponse.block)
            {
                case 0:
                    {
                        if (plugin.Config.VerboseMode)
                            Log.Debug($"{ipAddress} ({userID}) is not a detectable VPN.");
                        WhitelistAdd(ipAddress);
                        return false;
                    }

                case 1:
                    { 
                        if (plugin.Config.VerboseMode)
                            Log.Debug($"{ipAddress} ({userID}) is a detectable VPN. Kicking..");
                        BlacklistAdd(ipAddress);
                        return true;
                    }
                case 2:
                    {
                        if (plugin.Config.StrictBlocking)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"{ipAddress} ({userID}) is a detectable VPN (detected by strict blocking). Kicking..");
                            BlacklistAdd(ipAddress);
                            return true;
                        }
                        if (plugin.Config.VerboseMode)
                            Log.Debug($"{ipAddress} ({userID}) is not a detectable VPN.");
                        WhitelistAdd(ipAddress);
                        return false;
                    }
            }

            return false;
        }

        private static void WhitelistAdd(IPAddress ipAddress)
        {
            Plugin.vpnWhitelistedIPs.Add(ipAddress);
            using StreamWriter whitelist = File.AppendText($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt");
            whitelist.WriteLine(ipAddress);
        }

        private static void BlacklistAdd(IPAddress ipAddress)
        {
            Plugin.vpnBlacklistedIPs.Add(ipAddress);
            using StreamWriter blacklist = File.AppendText($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt");
            blacklist.WriteLine(ipAddress);
        }

        private bool WhitelistedIPCheck(IPAddress ipAddress, string userID)
        {
            if (!Plugin.vpnWhitelistedIPs.Contains(ipAddress))
                return false;
            if (plugin.Config.VerboseMode)
                Log.Debug($"{ipAddress} ({userID}) has already passed a VPN check / is whitelisted.");
            return true;
        }

        public bool BlacklistedIPCheck(IPAddress ipAddress, string userID)
        {
            if (!Plugin.vpnBlacklistedIPs.Contains(ipAddress))
                return false;
            if (plugin.Config.VerboseMode)
                Log.Debug($"{ipAddress} ({userID}) is already known as a VPN / is blacklisted. Kicking.");
            return true;
        }
    }
}
