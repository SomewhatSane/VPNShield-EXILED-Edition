using EXILED;

namespace VPNShield
{
    public class Plugin : EXILED.Plugin
    {
        public EventHandlers EventHandlers;
        public static string ipHubAPIKey;
        public static string vpnKickMessage;
        public static bool verboseMode;

        public override void OnEnable()
        {
            Plugin.Info("VPNShield EXILED Edition V1.0.1 by SomewhatSane. Last Modified: 2020/01/25 16:05 GMT.");
            Plugin.Info("Thanks to KarlOfDuty for the original SMod VPNShield!");
            Plugin.Info("Loading Configs.");

            ipHubAPIKey = Plugin.Config.GetString("vs_apikey", null);
            vpnKickMessage = Plugin.Config.GetString("vs_vpnkickmessage", "VPNs and proxies are forbidden on this server.");
            verboseMode = Plugin.Config.GetBool("vs_verbose", false);

            if (verboseMode)
            {
                Plugin.Info("Verbose mode is enabled.");
            }

            if (ipHubAPIKey == null)
            {
                Plugin.Info("This plugin requires an API Key! Get one for free at https://iphub.info, and set it to vs_apikey!");
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
