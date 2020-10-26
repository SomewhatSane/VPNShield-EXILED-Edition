using Exiled.API.Enums;
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
        public override Version RequiredExiledVersion { get; } = new Version("2.1.9");
        public override PluginPriority Priority { get; } = PluginPriority.Highest;

        public static readonly string exiledPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED/Plugins");


        public static readonly HashSet<IPAddress> vpnWhitelistedIPs = new HashSet<IPAddress>();
        public static readonly HashSet<IPAddress> vpnBlacklistedIPs = new HashSet<IPAddress>();
        public static readonly HashSet<string> accountWhitelistedUserIDs = new HashSet<string>();
        public static readonly HashSet<string> checksWhitelistedUserIDs = new HashSet<string>();


        internal const string version = "2.0.4";
        internal const string lastModifed = "2020/10/26 11:23 UTC";


        public override void OnEnabled()
        {
            if (!Config.IsEnabled) return;

            Log.Info($"{Name} v{version} by {Author}. Last Modified: {lastModifed}.");

            Log.Info("Loading base scripts.");
            Account = new Account(this);
            VPN = new VPN(this);

            if (Config.CheckForUpdates)
            {
                Log.Info("Checking for update.");
                _ = UpdateCheck.CheckForUpdate();
            }

            Log.Info("Running configuration validator.");
            Config.ConfigValidator();

            Log.Info("Checking file system.");
            Filesystem.CheckFileSystem();

            Log.Info("Loading data.");
            Filesystem.LoadData();

            Log.Info("Registering Event Handlers.");

            EventHandlers = new EventHandlers(this);
            PlayerEvents.PreAuthenticating += EventHandlers.PreAuthenticating;
            PlayerEvents.Joined += EventHandlers.Joined;
            ServerEvents.RoundEnded += EventHandlers.RoundEnded;
            ServerEvents.WaitingForPlayers += EventHandlers.WaitingForPlayers;

            Log.Info("Done.");
        }

        public override void OnDisabled()
        {
            PlayerEvents.PreAuthenticating -= EventHandlers.PreAuthenticating;
            PlayerEvents.Joined -= EventHandlers.Joined;
            ServerEvents.RoundEnded -= EventHandlers.RoundEnded;
            ServerEvents.WaitingForPlayers -= EventHandlers.WaitingForPlayers;
            EventHandlers = null;

            Account = null;
            VPN = null;

            Log.Info("Disabled.");
        }

        public override void OnReloaded() { }
    }
}
