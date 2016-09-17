using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Net.Sockets;
//using QueryMaster;
using System.Threading;
using System.Globalization;

namespace Squad.Admin.Rcon
{
    public class SquadRconConnection : SquadRconBase
    {
        ConnectionInfo conInfo;

        public SquadRconConnection(IPEndPoint endPoint, bool? isObsolete = false, int sendTimeout = 3000, int receiveTimeout = 3000, int retries = 3, bool throwExceptions = false)
        {
            conInfo = new ConnectionInfo
            {
                SendTimeout = sendTimeout,
                ReceiveTimeout = receiveTimeout,
                EndPoint = endPoint,
                Retries = retries,
                ThrowExceptions = throwExceptions
            };
        }

        public bool GetControl(string password)
        {
            return false;
            ThrowIfDisposed();
            bool isSuccess = false;
            //Rcon = RconSource.Authorize(ConInfo, pass);
            //if (Rcon != null)
            //    isSuccess = true;
            //return isSuccess;
        }
    }
}
