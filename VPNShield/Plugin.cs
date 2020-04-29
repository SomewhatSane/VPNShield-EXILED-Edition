using EXILED;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;

namespace VPNShield
{
    public class Plugin : EXILED.Plugin
    {
        public static string exiledPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Plugins");

        private EventHandlers eventHandlers;

        public static readonly HashSet<IPAddress> vpnWhitelistedIPs = new HashSet<IPAddress>();
        public static readonly HashSet<IPAddress> vpnBlacklistedIPs = new HashSet<IPAddress>();
        public static readonly HashSet<string> accountWhitelistedUserIDs = new HashSet<string>();
        public static readonly HashSet<string> checksWhitelistedUserIDs = new HashSet<string>();

        public static bool accountCheck;
        public static int minimumAccountAge;
        public static string accountCheckKickMessage;
        public static string steamAPIKey;

        public static bool vpnCheck;
        public static string ipHubAPIKey;
        public static string vpnKickMessage;
        public static string vpnKickMessageShort;

        public static bool verboseMode;
        public static bool updateChecker;

        public static readonly string version = "1.2.4";
        public static readonly string lastModifed = "2020/04/10 12:38 GMT";

        public override void OnEnable()
        {
            Log.Info("VPNShield EXILED Edition v" + version + " by SomewhatSane. Last Modified: " + lastModifed + ".");
            Log.Info("Thanks to KarlOfDuty for the original SMod VPNShield!");

            Log.Info("Loading configuration.");
            Setup.ReloadConfig();
            _ = UpdateCheck.CheckForUpdate();
            Log.Info("Checking file system.");
            Setup.CheckFileSystem();
            Log.Info("Loading data.");
            Setup.LoadData();

            Log.Info("Loading Event Handlers.");

            eventHandlers = new EventHandlers(this);
            Events.PreAuthEvent += eventHandlers.OnPreAuth;
            Events.PlayerJoinEvent += eventHandlers.OnPlayerJoin;
            Events.RoundEndEvent += eventHandlers.OnRoundEnd;
            Events.WaitingForPlayersEvent += eventHandlers.OnWaitingForPlayers;
            Events.RemoteAdminCommandEvent += eventHandlers.OnRACommand;

            Log.Info("Done.");
        }

        public override void OnDisable()
        {
            Events.PreAuthEvent -= eventHandlers.OnPreAuth;
            Events.PlayerJoinEvent -= eventHandlers.OnPlayerJoin;
            Events.RoundEndEvent -= eventHandlers.OnRoundEnd;
            Events.WaitingForPlayersEvent -= eventHandlers.OnWaitingForPlayers;
            Events.RemoteAdminCommandEvent -= eventHandlers.OnRACommand;
            eventHandlers = null;
            Log.Info("Disabled.");
        }

        public override void OnReload() { }

        public override string getName { get; } = "VPNShield EXILED Edition";
    }
}
