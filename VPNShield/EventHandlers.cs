using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using EXILED.Patches;
using LiteNetLib;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Mirror;
using LiteNetLib4Mirror;
using LiteNetLib.Utils;

namespace VPNShield
{
    public class EventHandlers
    {
        public Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        public void OnPlayerJoin(EXILED.PlayerJoinEvent ev)

        {
            
            if (VPN.CheckVPN(ev.Player.characterClassManager.connectionToClient.address, ev.Player.characterClassManager.UserId))
            {
                ServerConsole.Disconnect(ev.Player.characterClassManager.connectionToClient, Plugin.vpnKickMessage);
            }

            //Else, let them continue. UwU
        }
    }
}
