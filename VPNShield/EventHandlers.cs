using System.Threading.Tasks;
using EXILED;
using EXILED.Extensions;

namespace VPNShield
{
    public class EventHandlers
    {
        public Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        public void OnRACommand(ref EXILED.RACommandEvent ev)
        {
            if (ev.Command.ToUpper().Split(' ')[0] == "VS_RELOAD")
            {
                ev.Allow = false;
                Setup.ReloadConfig();
                Setup.LoadData();
                ev.Sender.RAMessage("Reloaded VPNShield.");
                Log.Info("Reloaded VPNShield.");
            }
        }

        public void OnPlayerJoin(EXILED.PlayerJoinEvent ev)
        {
            _ = Check(ev); //Do checks on in task to prevent holding up the game.
        }

        public async Task Check(EXILED.PlayerJoinEvent ev)
        {
            if (ev.Player.serverRoles.Staff)
            {
                if (Plugin.verboseMode)
                {
                    Log.Info("UserID " + ev.Player.characterClassManager.UserId + " (" + ev.Player.characterClassManager.connectionToClient.address + ") is a global SCPSL Staff Member. Bypassing..");
                }
                return; //Global staff. Bypass checks.
            }
            
            if (GlobalWhitelist.GlobalWhitelistCheck(ev.Player.characterClassManager.connectionToClient.address, ev.Player.characterClassManager.UserId)) //Check for globally whitelisted accounts.
            {
                return; //Quit all other checks. They are whitelisted.
            } 

            //Account check.
            if (Plugin.accountCheck && Plugin.steamAPIKey != null)
            {
                if (await Account.CheckAccount(ev.Player.characterClassManager.connectionToClient.address, ev.Player.characterClassManager.UserId))
                {
                    ServerConsole.Disconnect(ev.Player.characterClassManager.connectionToClient, Plugin.accountCheckKickMessage);
                    return;
                }
            }

            //VPN Check.
            if (Plugin.vpnCheck && Plugin.ipHubAPIKey != null)
            {
                if (await VPN.CheckVPN(ev.Player.characterClassManager.connectionToClient.address, ev.Player.characterClassManager.UserId))
                {
                    ServerConsole.Disconnect(ev.Player.characterClassManager.connectionToClient, Plugin.vpnKickMessage);
                    return;
                }
            }

            //Else, let them continue.
        }
        public void OnWaitingForPlayers()
        {
            Setup.ReloadConfig();
            Setup.LoadData();
        }
    }
}
