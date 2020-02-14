using EXILED;
using System.Collections.Generic;
using System;
using System.IO;

namespace VPNShield
{
    public class Plugin : EXILED.Plugin
    {
        private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string exiledPath = Path.Combine(appData, "Plugins");

        public EventHandlers EventHandlers;

        public static HashSet<string> vpnWhitelistedIPs;
        public static HashSet<string> vpnBlacklistedIPs;
        public static HashSet<string> accountWhitelistedUserIDs;
        public static HashSet<string> checksWhitelistedUserIDs;

        public static bool accountCheck;
        public static int minimumAccountAge;
        public static string accountCheckKickMessage;
        public static string steamAPIKey;

        public static bool vpnCheck;
        public static string ipHubAPIKey;
        public static string vpnKickMessage;

        public static bool verboseMode;
        public static bool updateChecker;

        public static string version = "1.2.2";

        public override void OnEnable()
        {
            Log.Info("VPNShield EXILED Edition v" + version + " by SomewhatSane. Last Modified: 2020/02/14 17:03 GMT.");
            Log.Info("Thanks to KarlOfDuty for the original SMod VPNShield!");

            Log.Info("Loading configs.");
            Setup.ReloadConfig();
            _ = UpdateCheck.CheckForUpdate();
            Log.Info("Checking file system.");
            Setup.CheckFileSystem();
            Log.Info("Loading data.");
            Setup.LoadData();
           

            Log.Info("Loading Event Handlers.");
            EventHandlers = new EventHandlers(this);
            Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
            Events.RemoteAdminCommandEvent += EventHandlers.OnRACommand;

            Log.Info("Done.");
            
        }

        public override void OnDisable()
        {
            Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
            EventHandlers = null;
            Log.Info("Disabled.");
        }

        public override void OnReload() { }

        public override string getName { get; } = "VPNShield EXILED Edition";
    }
}
