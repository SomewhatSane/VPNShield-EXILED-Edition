﻿using EXILED;

namespace VPNShield
{
    public class Plugin : EXILED.Plugin
    {
        public EventHandlers EventHandlers;

        public static bool accountCheck;
        public static int minimumAccountAge;
        public static string accountCheckKickMessage;
        public static string steamAPIKey;

        public static bool vpnCheck;
        public static string ipHubAPIKey;
        public static string vpnKickMessage;

        public static bool verboseMode;

        public override void OnEnable()
        {
            Plugin.Info("VPNShield EXILED Edition V1.1 by SomewhatSane. Last Modified: 2020/01/31 21:01 GMT.");
            Plugin.Info("Thanks to KarlOfDuty for the original SMod VPNShield!");
            Plugin.Info("Loading Configs.");


            accountCheck = Plugin.Config.GetBool("vs_accountcheck", false);
            steamAPIKey = Plugin.Config.GetString("vs_steamapikey", null);
            minimumAccountAge = Plugin.Config.GetInt("vs_accountminage", 14);
            accountCheckKickMessage = Plugin.Config.GetString("vs_accountkickmessage", "Your account must be atleast " + minimumAccountAge.ToString() + " day(s) old to play on this server.");

            vpnCheck = Plugin.Config.GetBool("vs_vpncheck", true);
            ipHubAPIKey = Plugin.Config.GetString("vs_vpnapikey", null);
            vpnKickMessage = Plugin.Config.GetString("vs_vpnkickmessage", "VPNs and proxies are forbidden on this server.");

            verboseMode = Plugin.Config.GetBool("vs_verbose", false);

            if (verboseMode)
            {
                Plugin.Info("Verbose mode is enabled.");
            }

            if (accountCheck && steamAPIKey == null)
            {
                Plugin.Info("This plugin requires a Steam API Key! Get one for free at https://steamcommunity.com/dev/apikey, and set it to vs_steamapikey!");
            }

            if (vpnCheck && ipHubAPIKey == null)
            {
                Plugin.Info("This plugin requires an VPN API Key! Get one for free at https://iphub.info, and set it to vs_vpnapikey!");
            }

            Plugin.Info("Checking File System.");
            Setup.CheckFileSystem();
            Plugin.Info("Loading Event Handlers.");

            EventHandlers = new EventHandlers(this);
            Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;

            Plugin.Info("Done.");
            
        }

        public override void OnDisable()
        {
            Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
            EventHandlers = null;
            Plugin.Info("Disabled.");
        }

        public override void OnReload() { }

        public override string getName { get; } = "VPNShield EXILED Edition";
    }
}
