using System;
using System.Net;

namespace Squad.AdminConsole.Utilities
{
    public class ServerConnection
    {
        public ServerConnection() { this.Password = string.Empty; this.AdminName = "RCON Admin"; }
        public IPAddress ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string Password { get; set; }
        public string AdminName { get; set; }

        public bool IsValid()
        {
            return this.ServerIP != null && this.ServerPort != 0 && this.Password.Trim() != string.Empty;
        }
    }
}
