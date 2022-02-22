using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace VPNShield
{
    public class Plugin : Plugin<Config>
    {
        public EventHandlers EventHandlers;
        public Account Account;
        public VPN VPN;
        public WebhookHandler WebhookHandler;

        public override string Name { get; } = "VPNShield EXILED Edition";
        public override string Author { get; } = "SomewhatSane";
        public override string Prefix { get; } = "vs";
        public override Version RequiredExiledVersion { get; } = new Version("5.0.0");

        public static readonly string exiledPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED"), "Plugins");

        public static readonly HashSet<IPAddress> vpnWhitelistedIPs = new();
        public static readonly HashSet<IPAddress> vpnBlacklistedIPs = new();
        public static readonly HashSet<string> accountAgeWhitelistedUserIDs = new();
        public static readonly HashSet<string> accountPlaytimeWhitelistedUserIDs = new();
        public static readonly HashSet<string> checksWhitelistedUserIDs = new();

        public static readonly string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        internal const string branch = "Stable";
        internal const string lastModifed = "2022/02/20 19:18 UTC";


        public override void OnEnabled()
        { 
            Log.Info($"{Name} {branch} v{version} by {Author}. Last modified: {lastModifed}.");

            Log.Info("Loading base scripts.");
            Account = new Account(this);
            VPN = new VPN(this);
            WebhookHandler = new WebhookHandler(this);

            if (Config.CheckForUpdates)
                _ = UpdateCheck.CheckForUpdate();

            Log.Info("Checking file system.");
            Filesystem.CheckFileSystem();

            Log.Info("Loading data.");
            Filesystem.LoadData();

            Log.Info("Registering Event Handlers.");

            EventHandlers = new EventHandlers(this);
            PlayerEvents.PreAuthenticating += EventHandlers.PreAuthenticating;
            PlayerEvents.Verified += EventHandlers.Verified;
            ServerEvents.RoundEnded += EventHandlers.RoundEnded;
            ServerEvents.WaitingForPlayers += EventHandlers.WaitingForPlayers;

            Log.Info("Done.");
        }

        public override void OnDisabled()
        {
            if (!Config.IsEnabled) return;

            PlayerEvents.PreAuthenticating -= EventHandlers.PreAuthenticating;
            PlayerEvents.Verified -= EventHandlers.Verified;
            ServerEvents.RoundEnded -= EventHandlers.RoundEnded;
            ServerEvents.WaitingForPlayers -= EventHandlers.WaitingForPlayers;
            EventHandlers = null;

            Account = null;
            VPN = null;

            Log.Info("Disabled.");
        }
    }
}
