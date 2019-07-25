using GSMasterServer.Data;
using GSMasterServer.Utils;
using Reality.Net.GameSpy.Servers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class LoginServer : Server
    {
        public const string Category = "Login";
        
        Thread _clientManagerThread;
        Thread _searchManagerThread;

        private static Socket _clientManagerSocket;
        private static Socket _searchManagerSocket;

        private readonly ManualResetEvent _clientManagerReset = new ManualResetEvent(false);
        private readonly ManualResetEvent _searchManagerReset = new ManualResetEvent(false);

        public LoginServer(IPAddress listen, ushort clientManagerPort, ushort searchManagerPort)
        {
            ServicePointManager.SetTcpKeepAlive(true, 60 * 1000 * 10, 1000);
            
            _clientManagerThread = new Thread(StartServerClientManager)
            {
                Name = "Login Thread Client Manager"
            };

            _clientManagerThread.Start(new AddressInfo()
            {
                Address = listen,
                Port = clientManagerPort
            });

            _searchManagerThread = new Thread(StartServerSearchManager)
            {
                Name = "Login Thread Search Manager"
            };

            _searchManagerThread.Start(new AddressInfo()
            {
                Address = listen,
                Port = searchManagerPort
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_clientManagerSocket != null)
                    {
                        _clientManagerSocket.Close();
                        _clientManagerSocket.Dispose();
                        _clientManagerSocket = null;
                    }
                    if (_searchManagerSocket != null)
                    {
                        _searchManagerSocket.Close();
                        _searchManagerSocket.Dispose();
                        _searchManagerSocket = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        ~LoginServer()
        {
            Dispose(false);
        }

        private void StartServerClientManager(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Log(Category, "Starting Login Server ClientManager");

            try
            {
                _clientManagerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 30000,
                    ReceiveTimeout = 30000,
                    SendBufferSize = 8192,
                    ReceiveBufferSize = 8192,
                    Blocking = false
                };

                _clientManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _clientManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
                _clientManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _clientManagerSocket.Bind(new IPEndPoint(info.Address, info.Port));
                _clientManagerSocket.Listen(10);
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Login Server ClientManager to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            while (true)
            {
                _clientManagerReset.Reset();

                LoginSocketState state = new LoginSocketState()
                {
                    Type = LoginSocketState.SocketType.Client,
                    Socket = _clientManagerSocket
                };

                _clientManagerSocket.BeginAccept(AcceptCallback, state);
                _clientManagerReset.WaitOne();
            }
        }

        private void StartServerSearchManager(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Log(Category, "Starting Login Server SearchManager");

            try
            {
                _searchManagerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = 8192,
                    ReceiveBufferSize = 8192,
                    Blocking = false
                };

                _searchManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _searchManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
                _searchManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _searchManagerSocket.Bind(new IPEndPoint(info.Address, info.Port));
                _searchManagerSocket.Listen(10);
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Login Server SearchManager to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            while (true)
            {
                _searchManagerReset.Reset();

                LoginSocketState state = new LoginSocketState()
                {
                    Type = LoginSocketState.SocketType.Search,
                    Socket = _searchManagerSocket
                };

                _searchManagerSocket.BeginAccept(AcceptCallback, state);
                _searchManagerReset.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            LoginSocketState state = (LoginSocketState)ar.AsyncState;

            try
            {
                Socket client = state.Socket.EndAccept(ar);

                Thread.Sleep(1);

                if (state.Type == LoginSocketState.SocketType.Client)
                    _clientManagerReset.Set();
                else if (state.Type == LoginSocketState.SocketType.Search)
                    _searchManagerReset.Set();

                state.Socket = client;

                Log(Category, String.Format("[{0}] New Client: {1}:{2}", state.Type, ((IPEndPoint)state.Socket.RemoteEndPoint).Address, ((IPEndPoint)state.Socket.RemoteEndPoint).Port));

                if (state.Type == LoginSocketState.SocketType.Client)
                {
                    // ClientManager server sends data first
                    byte[] buffer = LoginServerMessages.GenerateServerChallenge(ref state);
                    SendToClient(ref state, buffer);

                    if (state != null)
                    {
                        state.State++;
                    }
                }
                else if (state.Type == LoginSocketState.SocketType.Search)
                {
                    // SearchManager server waits for data first
                }
            }
            catch (NullReferenceException)
            {
                if (state != null)
                    state.Dispose();
                state = null;
            }
            catch (SocketException e)
            {
                LogError(Category, "Error accepting client");
                LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                if (state != null)
                    state.Dispose();
                state = null;
                return;
            }

            WaitForData(ref state);
        }

        public bool SendToClient(ref LoginSocketState state, byte[] data)
        {
            if (data == null || state == null || state.Socket == null)
                return false;

            Log("RESP", DataFunctions.BytesToString(data));

            try
            {
                state.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSent, state);
                return true;
            }
            catch (NullReferenceException)
            {
                if (state != null)
                    state.Dispose();
                state = null;
                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.ConnectionAborted &&
                    e.SocketErrorCode != SocketError.ConnectionReset)
                {
                    LogError(Category, "Error sending data");
                    LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                }
                if (state != null)
                    state.Dispose();
                state = null;
                return false;
            }
        }

        private void OnSent(IAsyncResult async)
        {
            LoginSocketState state = (LoginSocketState)async.AsyncState;

            if (state == null || state.Socket == null)
                return;

            try
            {
                int sent = state.Socket.EndSend(async);
                Log(Category, String.Format("[{0}] Sent {1} byte response to: {2}:{3}", state.Type, sent, ((IPEndPoint)state.Socket.RemoteEndPoint).Address, ((IPEndPoint)state.Socket.RemoteEndPoint).Port));
            }
            catch (NullReferenceException)
            {
                if (state != null)
                    state.Dispose();
                state = null;
            }
            catch (SocketException e)
            {
                switch (e.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                    case SocketError.Disconnecting:
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                    default:
                        LogError(Category, "Error sending data");
                        LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                }
            }
        }

        private void WaitForData(ref LoginSocketState state)
        {
            Thread.Sleep(10);

            try
            {
                state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceived, state);
            }
            catch (NullReferenceException)
            {
                if (state != null)
                    state.Dispose();
                state = null;
            }
            catch (ObjectDisposedException)
            {
                if (state != null)
                    state.Dispose();
                state = null;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.NotConnected)
                {
                    if (state != null)
                        state.Dispose();
                    state = null;
                    return;
                }

                if (e.SocketErrorCode != SocketError.ConnectionAborted &&
                    e.SocketErrorCode != SocketError.ConnectionReset)
                {
                    LogError(Category, "Error receiving data");
                    LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                }
                if (state != null)
                    state.Dispose();
                state = null;
                return;
            }
        }
        private void OnDataReceived(IAsyncResult async)
        {
            LoginSocketState state = (LoginSocketState)async.AsyncState;

            if (state == null || state.Socket == null)
                return;

            try
            {
                // receive data from the socket
                int received = state.Socket.EndReceive(async);
                if (received == 0)
                {
                    goto CONTINUE;
                }

                // take what we received, and append it to the received data buffer
                state.ReceivedData.Append(Encoding.UTF8.GetString(state.Buffer, 0, received));
                string receivedData = state.ReceivedData.ToString();

                // does what we received contain the \final\ delimiter?
                if (receivedData.LastIndexOf(@"\final\") > -1)
                {
                    state.ReceivedData.Clear();

                    // lets split up the message based on the delimiter
                    string[] messages = receivedData.Split(new string[] { @"\final\" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < messages.Length; i++)
                    {
                        ParseMessage(ref state, messages[i]);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                if (state != null)
                    state.Dispose();
                state = null;
                return;
            }
            catch (SocketException e)
            {
                switch (e.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                    case SocketError.Disconnecting:
                    case SocketError.NotConnected:
                    case SocketError.TimedOut:
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                    default:
                        LogError(Category, "Error receiving data");
                        LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                }
            }
            catch (Exception e)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, e.ToString());
            }

            // and we wait for more data...
            CONTINUE: WaitForData(ref state);
        }

        private void ParseMessage(ref LoginSocketState state, string message)
        {
            string query;
            Log("MESSAGE", message);

            var keyValues = GetKeyValue(message, out query);

            if (keyValues == null || String.IsNullOrWhiteSpace(query))
            {
                return;
            }

            if (keyValues.ContainsKey("name") && !keyValues.ContainsKey("email"))
            {
                var parts = keyValues["name"].Split('@');

                if (parts.Length > 2)
                {
                    keyValues["name"] = parts[0];
                    keyValues["email"] = parts[1]+"@"+ parts[2];
                }
            }

            Log(Category, String.Format("[{0}] Received {1} query from: {2}:{3}", state.Type, query, ((IPEndPoint)state.Socket.RemoteEndPoint).Address, ((IPEndPoint)state.Socket.RemoteEndPoint).Port));

            /*if (!keyValues.ContainsKey("ka"))
            {
				// say no to those not using bf2... Begone evil demon, bf2 for life!
				return;
			}*/

            switch (state.Type)
            {
                case LoginSocketState.SocketType.Client:
                    HandleClientManager(ref state, query, keyValues);
                    break;
                case LoginSocketState.SocketType.Search:
                    HandleSearchManager(ref state, query, keyValues);
                    break;
            }
        }

        private void HandleClientManager(ref LoginSocketState state, string query, Dictionary<string, string> keyValues)
        {
            if (state == null || String.IsNullOrWhiteSpace(query) || keyValues == null)
            {
                return;
            }
            
            if (state.State >= 4)
            {
                state.Dispose();
            }
            else
            {
                switch (query)
                {
                    case "login":
                        SendToClient(ref state, LoginServerMessages.SendProof(ref state, keyValues));
                        state.StartKeepAlive(this);

                        break;

                    case "newuser":
                        SendToClient(ref state, LoginServerMessages.NewUser(ref state, keyValues));
                        break;

                    case "getprofile":
                        SendToClient(ref state, LoginServerMessages.SendProfile(ref state, keyValues, false));
                        break;

                    case "updatepro":
                        LoginServerMessages.UpdateProfile(ref state, keyValues);
                        break;

                    case "status":

                        var remoteEndPoint = ((IPEndPoint)state.Socket.RemoteEndPoint);

                        // \status\1\sesskey\17562\statstring\DXP\locstring\-1

                        // userid\200000003\profileid\100000003\uniquenick\Bambochuk

                        //sendBuddies()
                         // SendToClient(ref state, $@"\bdy\1\list\100000003\final\".ToAssciiBytes());
                        //SendToClient(ref state, $@"\bdy\0\list\\final\".ToAssciiBytes());

                        // TODO: sendAddRequests();

                        //sendStatusUpdateToBuddies(this);

                        // send self status
                        // |s|%d|ss|%s%s%s|ip|%d|p|%d|qm|%d
                        /*
                        c->status,
		                c->statusstr,
		                c->locstr[0] != 0 ? "|ls|" : "",
		                c->locstr,
		                reverse_endian32(c->ip),
		                reverse_endian16(c->port),
		                c->quietflags
                        */

                        // SendToClient(ref state, $@"\bm\100\f\100000003\msg\|s|{2}|ss|DXP{"|ls|"}{-1}|ip|{(uint)IPAddress.NetworkToHostOrder((int)IPAddress.Loopback.Address)}|p|{ReverseEndian16(6500)}|qm|{0}\final\".ToAssciiBytes());
                        //SendToClient(ref state, $@"\bm\100\f\100000002\msg\|s|0|ss|Offline\final\".ToAssciiBytes());
                         //SendToClient(ref state, $@"\bm\100\f\100000001\msg\|s|{1}|ss|DXP{"|ls|"}{-1}|ip|{(uint)IPAddress.NetworkToHostOrder((int)remoteEndPoint.Address.Address)}|p|{ReverseEndian16(6500)}|qm|{0}\final\".ToAssciiBytes());



                        // send friend status



                        break;

                    case "logout":
                        LoginServerMessages.Logout(ref state, keyValues);
                        break;
                    case "registernick":
                        SendToClient(ref state, DataFunctions.StringToBytes(string.Format(@"\rn\{0}\id\{1}\final\", keyValues["uniquenick"], keyValues["id"])));
                        break;

                    case "ka":
                        SendToClient(ref state, $@"\ka\\final\".ToAssciiBytes());
                        break;

                    default:
                        break;
                }
            }
        }

        UInt32 ReverseEndian32(UInt32 x)
        { 
            //little to big or vice versa
            return (UInt32)(x << 24 | (x << 8 & 0x00ff0000) | x >> 8 & 0x0000ff00 | x >> 24 & 0x000000ff);
        }

        UInt16 ReverseEndian16(UInt16 x)
        { 
            //little to big or vice versa
            return (UInt16)((x & 0xff00) >> 8 | (x & 0x00ff) << 8);
        }

        private void HandleSearchManager(ref LoginSocketState state, string query, Dictionary<string, string> keyValues)
        {
            if (state.State == 0)
            {
                if (query.Equals("nicks", StringComparison.InvariantCultureIgnoreCase))
                {
                    SendToClient(ref state, LoginServerMessages.SendNicks(ref state, keyValues));
                }
                else if (query.Equals("check", StringComparison.InvariantCultureIgnoreCase))
                {
                    SendToClient(ref state, LoginServerMessages.SendCheck(ref state, keyValues));
                }
            }
            else if (state.State == 1)
            {
                state.State++;
            }
            else if (state.State >= 2)
            {
                state.Dispose();
            }
        }

        private static Dictionary<string, string> GetKeyValue(string message, out string query)
        {
            Dictionary<string, string> parsedData = new Dictionary<string, string>();

            string[] responseData = message.Split(new string[] { @"\" }, StringSplitOptions.None);

            if (responseData.Length > 1)
            {
                query = responseData[1];
            }
            else
            {
                query = String.Empty;
                return null;
            }

            for (int i = 1; i < responseData.Length - 1; i += 2)
            {
                if (parsedData.ContainsKey(responseData[i]))
                {
                    parsedData[responseData[i].ToLowerInvariant()] = responseData[i + 1];
                }
                else
                {
                    parsedData.Add(responseData[i].ToLowerInvariant(), responseData[i + 1]);
                }
            }

            return parsedData;
        }
    }

    internal class LoginSocketState : IDisposable
    {
        public enum SocketType
        {
            Client,
            Search
        }
        
        public SocketType Type;

        public Socket Socket = null;
        public byte[] Buffer = new byte[8192];
        public StringBuilder ReceivedData = new StringBuilder(8192);

        public int State = 0;
        public int HeartbeatState = 0;
        public string Session = "";

        public string ServerChallenge;
        public string ClientChallenge;
        public ulong SteamId;
        public string Name;
        public string Email;
        public string PasswordEncrypted;

        private Timer _keepAliveTimer;

        public void StartKeepAlive(LoginServer server)
        {
            if (_keepAliveTimer != null)
            {
                // if the timer already exists, destroy it so we can start a new one...
                _keepAliveTimer.Dispose();
            }

            // send a keep alive request every 2 minutes
            _keepAliveTimer = new Timer(KeepAliveCallback, server, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private void KeepAliveCallback(object s)
        {
            LoginServer server = (LoginServer)s;

            try
            {
                if (_keepAliveTimer == null)
                {
                    Dispose();
                    return;
                }

                LoginSocketState state = this;
                HeartbeatState++;

                Console.WriteLine("sending keep alive");
                if (!server.SendToClient(ref state, LoginServerMessages.SendKeepAlive()))
                {
                    Dispose();
                    return;
                }

                // every 2nd keep alive request, we send an additional heartbeat
                if (HeartbeatState % 2 == 0)
                {
                    Console.WriteLine("sending heartbeat");
                    if (!server.SendToClient(ref state, LoginServerMessages.SendHeartbeat()))
                    {
                        Dispose();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Server.LogError(LoginServer.Category, "Error running keep alive: " + e);
                Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (Socket != null)
                    {
                        Socket.Shutdown(SocketShutdown.Both);
                        Socket.Close();
                        Socket.Dispose();
                        Socket = null;
                    }

                    if (_keepAliveTimer != null)
                    {
                        _keepAliveTimer.Dispose();
                        _keepAliveTimer = null;
                    }
                }

                // yeah yeah, this is terrible, but it stops a memory leak :|
                GC.Collect();
            }
            catch (Exception)
            {
            }
        }

        ~LoginSocketState()
        {
            Dispose(false);
        }
    }
}
