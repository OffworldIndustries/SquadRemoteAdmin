using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using QueryMaster;
using QueryMaster.Steam;
using QueryMaster.GameServer;
using Squad.Admin.Console.Utilities;

namespace Squad.Admin.Console
{
    public class ServerProxy
    {
        private Server squadServer = null;
        private ServerConnectionInfo serverConnectionInfo = new ServerConnectionInfo();


        public ServerProxy()
        { }


        public bool Connect(ServerConnectionInfo connectionInfo)
        {
            this.serverConnectionInfo = connectionInfo;
            this.squadServer = ServerQuery.GetServerInstance(EngineType.Source, new IPEndPoint(this.serverConnectionInfo.ServerIP, this.serverConnectionInfo.ServerPort));
            return this.squadServer.GetControl(this.serverConnectionInfo.Password);
        }

        public void Disconnect()
        {
            this.squadServer.Dispose();
        }

        #region COMMANDS

        public string GetPlayerList()
        {
            return this.squadServer.Rcon.SendCommand("ListPlayers", true);
        }

        public string SendCommand(string command)
        {
            return this.squadServer.Rcon.SendCommand(command, false);
        }

        #endregion

    }
}
