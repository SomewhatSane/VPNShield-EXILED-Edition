using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace VPNShield
{
    public class Plugin : Plugin<Config>
    {
        public EventHandlers EventHandlers;
        public Account Account;
        public VPN VPN;

        public override string Name { get; } = "VPNShield EXILED Edition";
        public override string Author { get; } = "SomewhatSane";
        public override string Prefix { get; } = "vs";
        public override Version RequiredExiledVersion { get; } = new Version("2.8.0");

        public static readonly string exiledPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED/Plugins");

        public static readonly HashSet<IPAddress> vpnWhitelistedIPs = new HashSet<IPAddress>();
        public static readonly HashSet<IPAddress> vpnBlacklistedIPs = new HashSet<IPAddress>();
        public static readonly HashSet<string> accountWhitelistedUserIDs = new HashSet<string>();
        public static readonly HashSet<string> checksWhitelistedUserIDs = new HashSet<string>();


        internal const string version = "2.1.0";
        internal const string lastModifed = "2021/04/13 19:56 UTC";


        public override void OnEnabled()
        { 
            Log.Info($"{Name} v{version} by {Author}. Last modified: {lastModifed}.");

            Log.Info("Loading base scripts.");
            Account = new Account(this);
            VPN = new VPN(this);

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
