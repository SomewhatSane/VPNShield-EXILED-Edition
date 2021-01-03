using Exiled.API.Features;
using Exiled.API.Interfaces;
using System.ComponentModel;

namespace VPNShield
{
    public class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Should account age checking be enabled?")]
        public bool AccountCheck { get; private set; } = false;

        [Description("Should accounts that cannot be checked (eg, private Steam accounts) be kicked?")]
        public bool AccountKickPrivate { get; private set; } = true;

        [Description("Steam API key for account age checking.")]
        public string SteamApiKey { get; private set; } = null;

        [Description("Minimum Steam account age (if account checking is enabled).")]
        public int SteamMinAge { get; private set; } = 14;

        [Description("Message shown to players who are kicked by an account check.")]
        public string AccountCheckKickMessage { get; private set; } = "Your account must be at least 14 day(s) old to play on this server or your account age could not be checked due to privacy settings.";

        [Description("Should VPN checking be enabled?")]
        public bool VpnCheck { get; private set; } = true;

        [Description("IPHub.info API key for VPN checking.")]
        public string IpHubApiKey { get; private set; } = null;

        [Description("Message shown to players who are kicked by a VPN check.")]
        public string VpnKickMessage { get; private set; } = "VPNs and proxies are forbidden on this server.";

        [Description("Verbose mode. Prints more console messages with detailed information.")]
        public bool VerboseMode { get; private set; } = false;

        [Description("Check for VPNShield updates on startup?")]
        public bool CheckForUpdates { get; private set; } = true;

        public void ConfigValidator()
        {
            if (VerboseMode)
                Log.Info("Verbose mode is enabled.");

            Log.Info("Configuration loaded and validated.");
        }
    }
}
