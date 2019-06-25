using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using BF2Statistics.ASP;
using BF2Statistics.Gamespy.Net;
using BF2Statistics.Logging;
using BF2Statistics.Net;
using BF2Statistics.Utilities;

namespace BF2Statistics.Gamespy
{
    /// <summary>
    /// Master.Gamespy.com Server.
    /// Alot of code was borrowed and re-written from the Open Source PRMasterServer located
    /// here: https://github.com/AncientMan2002/PRMasterServer
    /// </summary>
    public class MasterServer : GamespyUdpSocket
    {
        /// <summary>
        /// Max number of concurrent open and active connections.
        /// </summary>
        /// <remarks>
        ///   While fast, the BF2Available requests will shoot out 6-8 times
        ///   per client while starting up BF2, so i set this alittle higher then usual.
        ///   Servers also post their data here, and alot of servers will keep the
        ///   connections rather high.
        /// </remarks>
        public const int MaxConnections = 16;

        /// <summary>
        /// The Server List Retrieve Tcp Socket
        /// </summary>
        private ServerListRetrieveSocket MasterTcpServer;

        /// <summary>
        /// BF2Available response
        /// </summary>
        private static readonly byte[] BF2AvailableReply = { 0xfe, 0xfd, 0x09, 0x00, 0x00, 0x00, 0x00 };
        
        /// <summary>
        /// BF2Available Message. 09 then 4 00's then battlefield2
        /// </summary>
        private static readonly byte[] BF2AvailableRequest = { 
                0x09, 0x00, 0x00, 0x00, 0x00, 0x62, 0x61, 0x74, 
                0x74, 0x6c, 0x65, 0x66, 0x69, 0x65, 0x6c, 0x64, 0x32, 0x00 
            };

        /// <summary>
        /// Our hardcoded Server Validation code
        /// </summary>
        private static readonly byte[] ServerValidateCode = { 
                0x72, 0x62, 0x75, 0x67, 0x4a, 0x34, 0x34, 0x64, 0x34, 0x7a, 0x2b, 
                0x66, 0x61, 0x78, 0x30, 0x2f, 0x74, 0x74, 0x56, 0x56, 0x46, 0x64, 
                0x47, 0x62, 0x4d, 0x7a, 0x38, 0x41, 0x00 
            };

        /// <summary>
        /// A List of all servers that have sent data to this master server, and are active in the last 30 seconds or so
        /// </summary>
        public static ConcurrentDictionary<string, GameServer> Servers = new ConcurrentDictionary<string, GameServer>();

        /// <summary>
        /// A timer that is used to Poll all the servers, and remove inactive servers from the server list
        /// </summary>
        private static Timer PollTimer;

        /// <summary>
        /// The Time for servers are to remain in the serverlist since the last ping.
        /// Once this value is surpassed, server is presumed offline and is removed
        /// </summary>
        public const int ServerTTL = 30;

        /// <summary>
        /// The Debug log (GamespyDebug.log)
        /// </summary>
        private LogWriter DebugLog;

        /// <summary>
        /// Event fires when a server is added or removed from the online serverlist
        /// </summary>
        public static event EventHandler OnServerlistUpdate;

        public MasterServer(ref int Port, LogWriter DebugLog) : base(27900, MaxConnections)
        {
            // Debugging
            this.DebugLog = DebugLog;
            DebugLog.Write("Bound to UDP port: " + Port);

            // === Start Server List Retrieve Tcp Socket
            Port = 28910;
            DebugLog.Write("Starting Server List Retrieve Socket");
            MasterTcpServer = new ServerListRetrieveSocket();
            DebugLog.Write("Bound to TCP port: " + Port);

            // Start accepting
            base.StartAcceptAsync();

            // Setup timer. Remove servers who havent ping'd since ServerTTL
            PollTimer = new Timer(5000);
            PollTimer.Elapsed += (s, e) => CheckServers();
            PollTimer.Start();
        }

        /// <summary>
        /// Shutsdown the Master server and socket
        /// </summary>
        public void Shutdown()
        {
            // Discard the poll timer
            PollTimer.Stop();
            PollTimer.Dispose();

            // Shutdown parent
            base.ShutdownSocket();
            MasterTcpServer.Shutdown();

            // Clear servers
            Servers.Clear();

            // Dispose parent objects
            base.Dispose();
        }

        #region Socket Callbacks

        /// <summary>
        /// Callback method for when the UDP Master socket recieves a connection
        /// </summary>
        protected override void ProcessAccept(GamespyUdpPacket Packet)
        {
            IPEndPoint remote = (IPEndPoint)Packet.AsyncEventArgs.RemoteEndPoint;

            // Need at least 5 bytes
            if (Packet.BytesRecieved.Length < 5)
            {
                base.Release(Packet.AsyncEventArgs);
                return;
            }

            // Handle request in a new thread
            Task.Run(() =>
            {
                // If we dont reply, we must manually release the pool
                bool replied = false;

                try
                {
                    // Both the clients and servers will send a Gamespy Available Heartbeat Check
                    // When starting Battlefield 2, the client will send 7 heartbeat checks
                    if (Packet.BytesRecieved[0] == 0x09 && Packet.BytesRecieved.SequenceEqual(BF2AvailableRequest))
                    {
                        DebugLog.Write("BF2Available Called From {0}:{1}", remote.Address, remote.Port);

                        // Send back a generic reply.
                        Packet.SetBufferContents(BF2AvailableReply);
                        base.ReplyAsync(Packet);
                        replied = true;
                    }
                    else if (Packet.BytesRecieved[0] == 0x03 && Program.Config.GamespyEnableServerlist)
                    {
                        // === this is where server details come in, it starts with 0x03, it happens every 60 seconds or so
                        // If we aren't validated (initial connection), send a challenge key
                        if (!ParseServerDetails(remote, Packet.BytesRecieved.Skip(5).ToArray()))
                        {
                            DebugLog.Write("Sending Server Challenge to {0}:{1}", remote.Address, remote.Port);

                            // this should be some sort of proper encrypted challenge, but for now i'm just going to hard code 
                            // it because I don't know how the encryption works...
                            byte[] uniqueId = Packet.BytesRecieved.Skip(1).Take(4).ToArray();
                            Packet.SetBufferContents(new byte[] 
                            { 
                                0xfe, 0xfd, 0x01, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3], 0x44, 0x3d, 0x73, 
                                0x7e, 0x6a, 0x59, 0x30, 0x30, 0x37, 0x43, 0x39, 0x35, 0x41, 0x42, 0x42, 0x35, 0x37, 0x34, 
                                0x43, 0x43, 0x00 
                            });
                            base.ReplyAsync(Packet);
                            replied = true;
                        }
                    }
                    else if (Packet.BytesRecieved[0] == 0x01 && Program.Config.GamespyEnableServerlist)
                    {
                        // === this is a challenge response, it starts with 0x01
                        if (Packet.BytesRecieved.Skip(5).SequenceEqual(ServerValidateCode))
                        {
                            DebugLog.Write("Server Challenge Recieved and Validated: {0}:{1}", remote.Address, remote.Port);

                            // Send back a good response if we validate successfully
                            if (ValidateServer(remote))
                            {
                                byte[] uniqueId = Packet.BytesRecieved.Skip(1).Take(4).ToArray();
                                Packet.SetBufferContents(new byte[] { 0xfe, 0xfd, 0x0a, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3] });
                                base.ReplyAsync(Packet);
                                replied = true;
                            }
                        }
                        else
                            DebugLog.Write("Server Challenge Received and FAILED Validation: {0}:{1}", remote.Address, remote.Port);
                    }
                    else if (Packet.BytesRecieved[0] == 0x08 && Program.Config.GamespyEnableServerlist)
                    {
                        // this is a server ping, it starts with 0x08, it happens every 20 seconds or so
                        string key = String.Format("{0}:{1}", remote.Address, remote.Port);
                        GameServer server;
                        if (Servers.TryGetValue(key, out server) && server.IsValidated)
                        {
                            DebugLog.Write("Server Heartbeat Received: " + key);

                            // Update Ping
                            server.LastPing = DateTime.Now;
                            Servers.AddOrUpdate(key, server, (k, old) => { return server; });
                        }
                        else 
                            DebugLog.Write("Server Heartbeat Received from Unvalidated Server: " + key);
                    }
                }
                catch (Exception E)
                {
                    L.LogError("ERROR: [MasterUdpServer.ProcessAccept] " + E.Message);
                }

                // Release so that we can pool the EventArgs to be used on another connection
                if (!replied)
                    base.Release(Packet.AsyncEventArgs);
            });
        }

        #endregion Socket Callbacks

        #region Support Methods

        /// <summary>
        /// When a server sends data initially, it needs to be validated with a validation code. 
        /// Once that has happened, this method is called, and it allows the server to be seen in the Serverlist.
        /// This method also corrects local IP addresses by converting them to External IP 
        /// addresses, so that external clients get a good IP to connect to.
        /// </summary>
        /// <param name="remote">The remote IP of the server</param>
        private bool ValidateServer(IPEndPoint remote)
        {
            string key = String.Format("{0}:{1}", remote.Address, remote.Port);
            GameServer server;

            // try to fetch the existing server, if its not here... we have bigger problems
            if (!Servers.TryGetValue(key, out server))
            {
                L.LogError("NOTICE: [MasterServer.ValidateServer] We encountered a strange error trying to fetch a connected server.");
                return false;
            }

            // Parse our External IP Address
            IPAddress ExtAddress = IPAddress.Loopback;
            IPAddress.TryParse(Program.Config.GamespyExtAddress, out ExtAddress);
            bool ExtAddressIsLocal = Networking.IsLanIP(ExtAddress);

            // Parse Server address and see if its external or LAN
            IPAddress serverAddress = server.AddressInfo.Address;
            bool isLocalServer = Networking.IsLanIP(server.AddressInfo.Address);

            // Check to make sure we allow external servers in our list
            if (!isLocalServer && !Program.Config.GamespyAllowExtServers)
            {
                DebugLog.Write("External Server not Allowed: " + key);
                return false;
            }
            else if (isLocalServer && !ExtAddressIsLocal)
                server.AddressInfo.Address = ExtAddress;

            // Server is valid
            server.IsValidated = true;
            server.LastRefreshed = DateTime.Now;
            server.LastPing = DateTime.Now;
            server.country = (serverAddress.AddressFamily == AddressFamily.InterNetwork)
                ? Ip2nation.GetCountryCode(serverAddress).ToUpperInvariant()
                : "??";

            // Update or add the new server
            DebugLog.Write("Adding Validated Server to Serverlist: " + key);
            Servers.AddOrUpdate(key, server, (k, old) => { return server; });

            // Fire the event
            if (OnServerlistUpdate != null)
                OnServerlistUpdate(null, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Executed every 60 seconds per server (Every 3rd ping), the BF2 server sends a full list
        /// of data that describes its current state, and this method is used to parse that
        /// data, and update the server in the Servers list
        /// </summary>
        /// <param name="remote">The servers remote address</param>
        /// <param name="data">The data we must parse, sent by the server</param>
        /// <returns>Returns whether or not the server needs to be validated, so it can be seen in the Server Browser</returns>
        private bool ParseServerDetails(IPEndPoint remote, byte[] data)
        {
            string key = String.Format("{0}:{1}", remote.Address, remote.Port);

            // split by 000 (info/player separator) and 002 (players/teams separator)
            // the players/teams separator is really 00, but because 00 may also be used elsewhere (an empty value for example), we hardcode it to 002
            // the 2 is the size of the teams, for BF2 this is always 2.
            string receivedData = Encoding.UTF8.GetString(data);
            string[] sections = receivedData.Split(new string[] { "\x00\x00\x00", "\x00\x00\x02" }, StringSplitOptions.None);
            if (sections.Length != 3 && !receivedData.EndsWith("\x00\x00"))
            {
                DebugLog.Write("Invalid Server Data Received From {0} :: {1}", key, sections[0]);
                return true; // true means we don't send back a response
            }

            // We only care about the server sections
            string serverVars = sections[0];
            string[] serverVarsSplit = serverVars.Split(new string[] { "\x00" }, StringSplitOptions.None);

            // Write to debug log
            DebugLog.Write("Server Data Received From {0}", key);
            for (int i = 0; i < sections.Length; i++)
                DebugLog.Write("    DataString {0}: {1}", i, sections[i]);

            // Start a new Server Object, and assign its BF2 server properties
            GameServer server = new GameServer(remote);
            for (int i = 0; i < serverVarsSplit.Length - 1; i += 2)
            {
                // Fetch the property
                PropertyInfo property = server.GetType().GetProperty(serverVarsSplit[i]);
                if (property == null)
                    continue;
                else if (property.Name == "hostname")
                {
                    // strip consecutive whitespace from hostname
                    property.SetValue(server, Regex.Replace(serverVarsSplit[i + 1], @"\s+", " ").Trim(), null);
                }
                else if (property.Name == "bf2_plasma")
                {
                    property.SetValue(server, false, null);
                }
                else if (property.Name == "bf2_ranked")
                {
                    // we're always a ranked server (helps for mods with a default bf2 main menu, and default filters wanting ranked servers)
                    property.SetValue(server, true, null);
                }
                else if (property.Name == "bf2_pure")
                {
                    // we're always a pure server
                    property.SetValue(server, true, null);
                }
                else if (property.PropertyType == typeof(Boolean))
                {
                    // parse string to bool (values come in as 1 or 0)
                    int value;
                    if (Int32.TryParse(serverVarsSplit[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    {
                        property.SetValue(server, value != 0, null);
                    }
                }
                else if (property.PropertyType == typeof(Int32))
                {
                    // parse string to int
                    int value;
                    if (Int32.TryParse(serverVarsSplit[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    {
                        property.SetValue(server, value, null);
                    }
                }
                else if (property.PropertyType == typeof(Double))
                {
                    // parse string to double
                    double value;
                    if (Double.TryParse(serverVarsSplit[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    {
                        property.SetValue(server, value, null);
                    }
                }
                else if (property.PropertyType == typeof(String))
                {
                    // parse string to string
                    property.SetValue(server, serverVarsSplit[i + 1], null);
                }
            }

            // you've got to have all these properties in order for your server to be valid
            if (!String.IsNullOrWhiteSpace(server.hostname) &&
                !String.IsNullOrWhiteSpace(server.gamevariant) &&
                !String.IsNullOrWhiteSpace(server.gamever) &&
                !String.IsNullOrWhiteSpace(server.gametype) &&
                !String.IsNullOrWhiteSpace(server.mapname) &&
                !String.IsNullOrWhiteSpace(server.gamename) &&
                server.gamename.Equals("battlefield2", StringComparison.InvariantCultureIgnoreCase) &&
                server.hostport > 1024 && server.hostport <= UInt16.MaxValue &&
                server.maxplayers > 0)
            {
                // Determine if we need to send a challenge key to the server for validation
                GameServer oldServer;
                bool IsValidated = Servers.TryGetValue(key, out oldServer) && oldServer.IsValidated;
                DebugLog.Write("Server Data Parsed Successfully... Needs Validated: " + ((IsValidated) ? "false" : "true"));

                // Copy over the local lan fix if we already have in the past
                if (IsValidated)
                {
                    server.AddressInfo.Address = oldServer.AddressInfo.Address;
                    server.country = oldServer.country;
                }

                // Add / Update Server
                server.IsValidated = IsValidated;
                server.LastPing = DateTime.Now;
                server.LastRefreshed = DateTime.Now;
                Servers.AddOrUpdate(key, server, (k, old) => { return server; });

                // Tell the requester if we are good to go
                return IsValidated;
            }

            // If we are here, the server information is partial/invalid. Return true to ignore server
            // until we can get some good data. 
            DebugLog.Write("Data from {0} was partial. Assuming server switched game status. Complete data expected in 10 seconds.", key);
            return true;
        }

        /// <summary>
        /// Executed every 5 seconds or so... Removes all servers that haven't
        /// reported in awhile, assuming they are offline
        /// </summary>
        private void CheckServers()
        {
            bool removed = false;

            // Run this on all available processors
            foreach (KeyValuePair<string, GameServer> Server in Servers)
            {
                if (Server.Value.LastPing < DateTime.Now - TimeSpan.FromSeconds(ServerTTL))
                {
                    DebugLog.Write("Removing Server for Expired Ping: " + Server.Key);

                    GameServer value;
                    if (Servers.TryRemove(Server.Key, out value) && !removed)
                        removed = true;
                }
            }

            // Fire the event
            if (removed && OnServerlistUpdate != null)
                OnServerlistUpdate(null, EventArgs.Empty);
        }

        #endregion Support Methods
    }
}
