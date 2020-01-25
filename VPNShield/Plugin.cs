using EXILED;

namespace VPNShield
{
    public class Plugin : EXILED.Plugin
    {
        public EventHandlers EventHandlers;

        public static string bypassVPNUsers;
        public static string ipHubAPIKey;
        public static string vpnKickMessage;
        public static bool verboseMode;
        public static int verificationLevel;

        public override void OnEnable()
        {
            Plugin.Info("VPNShield EXILED Edition V1.0 - By SomewhatSane. Last Modified: 2020/01/25 13:49 GMT.");
            Plugin.Info("Thanks to KarlOfDuty for the original SMod VPNShield!");

            Debug("VPNShield is starting up. Loading Configs.");
            Plugin.Info("Loading Configs.");

            ipHubAPIKey = Plugin.Config.GetString("vs_apikey", null);
            vpnKickMessage = Plugin.Config.GetString("vs_vpnkickmessage", "VPNs and proxies are forbidden on this server.");
            verboseMode = Plugin.Config.GetBool("vs_verbose", false);

            if (verboseMode)
            {
                Debug("Verbose mode is enabled.");
                Plugin.Info("Verbose mode is enabled.");
            }

            if (ipHubAPIKey == null)
            {
                Debug("Server does not have a IPHub API Key.");
                Plugin.Info("This plugin requires an API Key! Get one for free at https://iphub.info, and set it to vs_apiKey!");
            }

            Debug("VPNShield is starting up. Checking File System.");
            Plugin.Info("Checking File System.");

            Setup.CheckFileSystem();

            Debug("VPNShield is starting up. Loading Event Handlers.");
            Plugin.Info("Loading Event Handlers.");


            EventHandlers = new EventHandlers(this);
            Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;


            Debug("VPNShield has been started, and is ready to go.");
            Plugin.Info("Done.");
            
        }

        public override void OnDisable()
        {
            Debug("VPNShield shutting down. Shutting down Event Handlers.");
            Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
            EventHandlers = null;
            Plugin.Info("Disabled.");
        }

        public override void OnReload()
        {
           
        }

        public override string getName { get; } = "VPNShield EXILED Edition";
    }
}
