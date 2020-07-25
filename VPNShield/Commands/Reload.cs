using CommandSystem;
using RemoteAdmin;
using System;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Reload : ICommand
    {
        public string Command { get; } = "vs_reload";

        public string[] Aliases { get; } = { "vs_r" };
        public string Description { get; } = "Reload VPNShield's data.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Filesystem.CheckFileSystem();
            Filesystem.LoadData();

            response = "Reloaded VPNShield.";
            return true;
        }
    }
}
