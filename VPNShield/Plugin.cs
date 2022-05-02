using Exiled.API.Features;
using System;
using System.IO;
using System.Reflection;
using VPNShield.API;
using VPNShield.Handlers;
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

        public static readonly string exiledPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED", "Plugins");

        public static readonly string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        internal const string lastModifed = "2022/05/01 10:22 UTC";


        public override void OnEnabled()
        { 
            Log.Info($"{Name} v{version} by {Author}. Last modified: {lastModifed}.");

            Log.Info("Loading base scripts.");
            Account = new Account(this);
            VPN = new VPN(this);
            WebhookHandler = new WebhookHandler(this);

            if (Config.CheckForUpdates)
                _ = UpdateCheck.CheckForUpdate();

            Log.Info("Checking file system.");
            Filesystem.CheckFileSystem();

            Log.Info("Registering Event Handlers.");

            EventHandlers = new EventHandlers(this);
            PlayerEvents.PreAuthenticating += EventHandlers.PreAuthenticating;
            PlayerEvents.Verified += EventHandlers.Verified;
            ServerEvents.WaitingForPlayers += EventHandlers.WaitingForPlayers;
            ServerEvents.RoundEnded += EventHandlers.RoundEnded;

            Log.Info("Done.");
        }

        public override void OnDisabled()
        {
            if (!Config.IsEnabled) return;

            PlayerEvents.PreAuthenticating -= EventHandlers.PreAuthenticating;
            PlayerEvents.Verified -= EventHandlers.Verified;
            ServerEvents.WaitingForPlayers -= EventHandlers.WaitingForPlayers;
            ServerEvents.RoundEnded -= EventHandlers.RoundEnded;

            EventHandlers = null;
            Account = null;
            VPN = null;

            Log.Info("Disabled.");
        }
    }
}
