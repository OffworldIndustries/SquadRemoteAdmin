using System;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;

namespace Squad.Admin.Console.RCON
{
    /// <summary>
    /// Summary description for SourceRcon.
    /// </summary>
    public class RconConnection
    {
        public event CommandOutput ServerResponseReceived;
        public event ErrorOutput ServerError;
        public event BoolInfo ConnectionSuccess;

        public static string ConnectionClosed = "Connection closed by remote host";
        public static string ConnectionSuccessString = "Connection Succeeded!";
        public static string ConnectionFailedString = "Connection Failed!";
        public static string UnknownResponseType = "Unknown response";
        public static string GotJunkPacket = "Had junk packet. This is normal.";

        internal Socket S;
        bool connected;


        private string commandName = string.Empty;
        private readonly object LockObj = new object();

        private byte[] EmptyPkt = new byte[] { 0x0a, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public RconConnection()
        {
            S = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            PacketCount = 0;

#if DEBUG
			TempPackets = new ArrayList();
#endif
        }

        public bool Connect(IPEndPoint Server, string password)
        {
            try
            {
                S.Connect(Server);
            }
            catch (SocketException)
            {
                OnError(ConnectionFailedString);
                OnConnectionSuccess(false);
                return false;
            }

            RCONPacket SA = new RCONPacket();
            SA.RequestId = 1;
            SA.String1 = password;
            SA.ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_AUTH;

            SendRCONPacket(SA);

            // This is the first time we've sent, so we can start listening now!
            StartGetNewPacket();

            return true;
        }

        public void Disconnect()
        {
            if (S.Connected)
            {
                S.Close();
            }
        }

        public void ServerCommand(string command)
        {
            if (connected)
            {
                RCONPacket PacketToSend = new RCONPacket();
                PacketToSend.RequestId = 2;
                PacketToSend.ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_EXECCOMMAND;
                PacketToSend.String1 = command;
                this.commandName = GetCommandName(command);     // This is stored in state so the command name can be returned with the response
                SendRCONPacket(PacketToSend);
            }
        }

        /// <summary>
        /// Strip off any arguments provided with the command and just return the command name , like "AdminBanPlayerById", "ListPlayers", etc.
        /// </summary>
        /// <param name="command">The full text of the command</param>
        /// <returns>Just the name of the command to be executed</returns>
        private string GetCommandName(string command)
        {
            string cmd = command.Trim();

            if (command.IndexOf(" ", 0) > 0)
            {
                cmd = command.Substring(0, command.IndexOf(" ", 0)).Trim();
            }

            return cmd;
        }

        void SendRCONPacket(RCONPacket p)
        {
            byte[] Packet = p.OutputAsBytes();
            S.BeginSend(Packet, 0, Packet.Length, SocketFlags.None, new AsyncCallback(SendCallback), this);
        }


        public bool Connected
        {
            get { return connected; }
        }

        void SendCallback(IAsyncResult ar)
        {
            S.EndSend(ar);
        }

        int PacketCount;

        void StartGetNewPacket()
        {
            RecState state = new RecState();
            state.IsPacketLength = true;
            state.Data = new byte[4];
            state.PacketCount = PacketCount;
            PacketCount++;
#if DEBUG
            TempPackets.Add(state);
#endif
            S.BeginReceive(state.Data, 0, 4, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
        }

#if DEBUG
        public ArrayList TempPackets;
#endif

        void ReceiveCallback(IAsyncResult ar)
        {
            bool recsuccess = false;
            RecState state = null;

            try
            {
                int bytesgotten = S.EndReceive(ar);
                state = (RecState)ar.AsyncState;
                state.BytesSoFar += bytesgotten;
                recsuccess = true;

#if DEBUG
                Trace.WriteLine(String.Format("Receive Callback. Packet: {0} First packet: {1}, Bytes so far: {2}", state.PacketCount, state.IsPacketLength, state.BytesSoFar));
#endif

            }
            catch (SocketException)
            {
                OnError(ConnectionClosed);
            }

            if (recsuccess)
                ProcessIncomingData(state);
        }

        void ProcessIncomingData(RecState state)
        {
            if (state.IsPacketLength)
            {
                // First 4 bytes of a new packet.
                state.PacketLength = BitConverter.ToInt32(state.Data, 0);

                state.IsPacketLength = false;
                state.BytesSoFar = 0;
                state.Data = new byte[state.PacketLength];
                S.BeginReceive(state.Data, 0, state.PacketLength, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // Do something with data...

                if (state.BytesSoFar < state.PacketLength)
                {
                    // Missing data.
                    S.BeginReceive(state.Data, state.BytesSoFar, state.PacketLength - state.BytesSoFar, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // Process data.
#if DEBUG
                    Trace.WriteLine("Complete packet.");
#endif

                    RCONPacket RetPack = new RCONPacket();
                    RetPack.ParseFromBytes(state.Data, this);

                    ProcessResponse(RetPack);

                    // Wait for new packet.
                    StartGetNewPacket();
                }
            }
        }

        void ProcessResponse(RCONPacket P)
        {
            switch (P.ServerDataReceived)
            {
                case RCONPacket.SERVERDATA_rec.SERVERDATA_AUTH_RESPONSE:
                    if (P.RequestId != -1)
                    {
                        // Connected.
                        connected = true;
                        OnError(ConnectionSuccessString);
                        OnConnectionSuccess(true);
                    }
                    else
                    {
                        // Failed!
                        OnError(ConnectionFailedString);
                        OnConnectionSuccess(false);
                    }
                    break;
                case RCONPacket.SERVERDATA_rec.SERVERDATA_RESPONSE_VALUE:
                    if (hadjunkpacket)
                    {
                        // Real packet!
                        OnServerOutput(this.commandName, P.String1);
                    }
                    else
                    {
                        hadjunkpacket = true;
                        //OnError(GotJunkPacket);
                        OnError(string.Empty);
                    }
                    break;
                default:
                    OnError(UnknownResponseType);
                    break;
            }
        }

        bool hadjunkpacket;

        internal void OnServerOutput(string command, string output)
        {
            if (ServerResponseReceived != null)
            {
                ServerResponseReceived(command, output);
            }
        }

        internal void OnError(string error)
        {
            if (ServerError != null)
            {
                ServerError(error);
            }
        }

        internal void OnConnectionSuccess(bool info)
        {
            if (ConnectionSuccess != null)
            {
                ConnectionSuccess(info);
            }
        }

    }

    public delegate void CommandOutput(string commandName, string commandResponse);
    public delegate void ErrorOutput(string error);
    public delegate void BoolInfo(bool info);

    internal class RecState
    {
        internal RecState()
        {
            PacketLength = -1;
            BytesSoFar = 0;
            IsPacketLength = false;
        }

        public int PacketCount;
        public int PacketLength;
        public int BytesSoFar;
        public bool IsPacketLength;
        public byte[] Data;
    }



    internal class RCONPacket
    {
        internal RCONPacket()
        {
            RequestId = 0;
            String1 = "blah";
            String2 = String.Empty;
            ServerDataSent = SERVERDATA_sent.None;
            ServerDataReceived = SERVERDATA_rec.None;
        }

        internal byte[] OutputAsBytes()
        {
            byte[] packetsize;
            byte[] reqid;
            byte[] serverdata;
            byte[] bstring1;
            byte[] bstring2;

            UTF8Encoding utf = new UTF8Encoding();

            bstring1 = utf.GetBytes(String1);
            bstring2 = utf.GetBytes(String2);

            serverdata = BitConverter.GetBytes((int)ServerDataSent);
            reqid = BitConverter.GetBytes(RequestId);

            // Compose into one packet.
            byte[] FinalPacket = new byte[4 + 4 + 4 + bstring1.Length + 1 + bstring2.Length + 1];
            packetsize = BitConverter.GetBytes(FinalPacket.Length - 4);

            int BPtr = 0;
            packetsize.CopyTo(FinalPacket, BPtr);
            BPtr += 4;

            reqid.CopyTo(FinalPacket, BPtr);
            BPtr += 4;

            serverdata.CopyTo(FinalPacket, BPtr);
            BPtr += 4;

            bstring1.CopyTo(FinalPacket, BPtr);
            BPtr += bstring1.Length;

            FinalPacket[BPtr] = (byte)0;
            BPtr++;

            bstring2.CopyTo(FinalPacket, BPtr);
            BPtr += bstring2.Length;

            FinalPacket[BPtr] = (byte)0;
            BPtr++;

            return FinalPacket;
        }

        internal void ParseFromBytes(byte[] bytes, RconConnection parent)
        {

            File.WriteAllBytes(@"D:\Projects\OWI\listplayers_70players", bytes);

            if (bytes.Length == 0) return;

            int BPtr = 0;
            ArrayList stringcache;
            UTF8Encoding utf = new UTF8Encoding();

            // First 4 bytes are ReqId.
            RequestId = BitConverter.ToInt32(bytes, BPtr);
            BPtr += 4;
            // Next 4 are server data.
            ServerDataReceived = (SERVERDATA_rec)BitConverter.ToInt32(bytes, BPtr);
            BPtr += 4;
            // string1 till /0
            stringcache = new ArrayList();

            while (bytes[BPtr] != 0)
            {
                stringcache.Add(bytes[BPtr]);
                BPtr++;
            }
            String1 = utf.GetString((byte[])stringcache.ToArray(typeof(byte)));
            BPtr++;

            // string2 till /0

            stringcache = new ArrayList();
            while (bytes[BPtr] != 0)
            {
                stringcache.Add(bytes[BPtr]);
                BPtr++;
            }
            String2 = utf.GetString((byte[])stringcache.ToArray(typeof(byte)));
            BPtr++;

            // Repeat if there's more data?

            if (BPtr != bytes.Length)
            {
                parent.OnError(String.Format("Unexpected value in return packet encountered! Value '{0}' encountered. Processing of response has been terminated.", bytes[BPtr].ToString()));
            }
        }

        public enum SERVERDATA_sent : int
        {
            SERVERDATA_RESPONSE_VALUE = 0,
            SERVERDATA_AUTH = 3,
            SERVERDATA_EXECCOMMAND = 2,
            None = 255
        }

        public enum SERVERDATA_rec : int
        {
            SERVERDATA_RESPONSE_VALUE = 0,
            SERVERDATA_AUTH_RESPONSE = 2,
            None = 255
        }

        internal int RequestId;
        internal string String1;
        internal string String2;
        internal RCONPacket.SERVERDATA_sent ServerDataSent;
        internal RCONPacket.SERVERDATA_rec ServerDataReceived;
    }
}
