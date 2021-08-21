using Exiled.API.Interfaces;
using System.ComponentModel;

namespace VPNShield
{
    public class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Should account age checking be enabled?")]
        public bool AccountAgeCheck { get; private set; } = false;

        [Description("Should account playtime checking be enabled?")]
        public bool AccountPlaytimeCheck { get; private set; } = false;

        [Description("Should accounts that cannot be checked (eg. private Steam accounts) be kicked?")]
        public bool AccountKickPrivate { get; private set; } = true;

        [Description("Should accounts that cannot be checked due to a Steam API error be kicked? In most cases, you should keep this set to false.")]
        public bool AccountKickOnSteamError { get; private set; } = false;

        [Description("Steam API key for account age checking.")]
        public string SteamApiKey { get; private set; } = null;

        [Description("Minimum Steam account age (if account age checking is enabled - in days).")]
        public int SteamMinAge { get; private set; } = 14;

        [Description("Minimum required SCPSL playtime required (if account playtime checking is enabled - in minutes).")]
        public int SteamMinPlaytime { get; private set; } = 0;

        [Description("Message shown to players who are kicked by an account age check. You may use %MINIMUMAGE% to insert the minimum age in days set into your kick message.")]
        public string AccountAgeCheckKickMessage { get; private set; } = "Your account must be at least %MINIMUMAGE% day(s) old to play on this server.";

        [Description("Message shown to players who are kicked by an account playtime check. You may use %MINIMUMPLAYTIME% to insert the minimum playtime in minutes set into your kick message.")]
        public string AccountPlaytimeCheckKickMessage { get; private set; } = "Your account must have played SCP: SL for atleast %MINIMUMPLAYTIME% minute(s) to play on this server.";

        [Description("Message shown to players who are kicked because they account cannot be checked due to privacy settings.")]
        public string AccountPrivateKickMessage { get; private set; } = "An account check could not be performed as your profile is set to private. Please make your profile public and try connecting again!";

        [Description("Message shown to players who are kicked as there was a Steam API error (only needed if account_kick_on_steam_error).")]
        public string AccountSteamErrorKickMessage { get; private set; } = "An error occurred when trying to check your account. Due to the policy set on the server, you were kicked. Please contact the server administrator about this and try again later.";

        [Description("Should VPN checking be enabled?")]
        public bool VpnCheck { get; private set; } = true;

        [Description("IPHub API key for VPN checking. Get one for free at https://iphub.info .")]
        public string IpHubApiKey { get; private set; } = null;

        [Description("Should strict blocking be enabled? Strict blocking will catch more VPN / hosting IP addresses but may cause false positives. It is generally recommended to keep this disabled.")]
        public bool StrictBlocking { get; private set; } = false;

        [Description("Message shown to players who are kicked by a VPN check.")]
        public string VpnKickMessage { get; private set; } = "VPNs and proxies are forbidden on this server.";

        [Description("Check for VPNShield updates on startup?")]
        public bool CheckForUpdates { get; private set; } = true;

        [Description("Verbose mode. Prints more console messages.")]
        public bool VerboseMode { get; private set; } = false;
    }
}
