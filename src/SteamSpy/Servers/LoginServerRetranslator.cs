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
    internal class LoginServerRetranslator : Server
    {
        public const string Category = "Login Server Retranslator";
        
        Thread _clientManagerThread;
        Thread _searchManagerThread;

        private static Socket _clientManagerSocket;
        private static Socket _searchManagerSocket;

        private static Socket _clientManagerServerSocket;
        private static Socket _searchManagerServerSocket;

        private readonly ManualResetEvent _clientManagerReset = new ManualResetEvent(false);
        private readonly ManualResetEvent _searchManagerReset = new ManualResetEvent(false);
        
        public LoginServerRetranslator(IPAddress listen, ushort clientManagerPort, ushort searchManagerPort)
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
                    Log(Category, "DISPOSING");

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

                    if (_searchManagerServerSocket != null)
                    {
                        _searchManagerServerSocket.Close();
                        _searchManagerServerSocket.Dispose();
                        _searchManagerServerSocket = null;
                    }
                    
                    if (_clientManagerServerSocket != null)
                    {
                        _clientManagerServerSocket.Close();
                        _clientManagerServerSocket.Dispose();
                        _clientManagerServerSocket = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        ~LoginServerRetranslator()
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
                _clientManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _clientManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                _clientManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _clientManagerServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 30000,
                    ReceiveTimeout = 30000,
                    SendBufferSize = 8192,
                    ReceiveBufferSize = 8192,
                    Blocking = false
                };

                _clientManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _clientManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _clientManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                _clientManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _clientManagerSocket.Bind(new IPEndPoint(info.Address, info.Port));
                _clientManagerServerSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
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
                    GameSocket = _clientManagerSocket
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
                _searchManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _searchManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                _searchManagerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _searchManagerServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = 8192,
                    ReceiveBufferSize = 8192,
                    Blocking = false
                };
                _searchManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _searchManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _searchManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                _searchManagerServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);


                _searchManagerSocket.Bind(new IPEndPoint(info.Address, info.Port));
                _searchManagerServerSocket.Bind(new IPEndPoint(IPAddress.Any, 0));

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
                    GameSocket = _searchManagerSocket
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
                Socket client = state.GameSocket.EndAccept(ar);

                Thread.Sleep(1);

                if (state.Type == LoginSocketState.SocketType.Client)
                    _clientManagerReset.Set();
                else if (state.Type == LoginSocketState.SocketType.Search)
                    _searchManagerReset.Set();

                state.GameSocket = client;
                
                if (state.Type == LoginSocketState.SocketType.Client)
                {
                    state.ServerSocket = _clientManagerServerSocket;
                    _clientManagerServerSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("134.209.198.2"), 29900), OnConnect, state);
                    // ClientManager server sends data first
                   // byte[] buffer = LoginServerMessages.GenerateServerChallenge(ref state);
                   // SendToClient(ref state, buffer);
                   
                }
                else if (state.Type == LoginSocketState.SocketType.Search)
                {
                    state.ServerSocket = _searchManagerServerSocket;
                    _searchManagerServerSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("134.209.198.2"), 29901), OnConnect, state);
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
        }

        private void OnConnect(IAsyncResult ar)
        {
            var state = (LoginSocketState)ar.AsyncState;

            state.ServerSocket.EndConnect(ar);

            WaitForServerData(ref state);
            WaitForData(ref state);
        }

        public bool SendToServer(ref LoginSocketState state, byte[] data, int count)
        {
            if (data == null || state == null || state.ServerSocket == null)
                return false;

            //Log("RESP", DataFunctions.BytesToString(data));

            try
            {
                state.ServerSocket.BeginSend(data, 0, count, SocketFlags.None, OnToServerSent, state);
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

        public bool SendToGame(ref LoginSocketState state, byte[] data, int count)
        {
            if (data == null || state == null || state.GameSocket == null)
                return false;

            Log("RESP", DataFunctions.BytesToString(data));

            try
            {
                state.GameSocket.BeginSend(data, 0, count, SocketFlags.None, OnToGameSent, state);
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

        private void OnToServerSent(IAsyncResult async)
        {
            LoginSocketState state = (LoginSocketState)async.AsyncState;

            if (state == null || state.ServerSocket == null)
                return;

            try
            {
                int sent = state.ServerSocket.EndSend(async);
                //Log(Category, String.Format("[{0}] Sent {1} byte response to: {2}:{3}", state.Type, sent, ((IPEndPoint)state.GameSocket.RemoteEndPoint).Address, ((IPEndPoint)state.GameSocket.RemoteEndPoint).Port));
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

        private void OnToGameSent(IAsyncResult async)
        {
            LoginSocketState state = (LoginSocketState)async.AsyncState;

            if (state == null || state.GameSocket == null)
                return;

            try
            {
                int sent = state.GameSocket.EndSend(async);
                //Log(Category, String.Format("[{0}] Sent {1} byte response to: {2}:{3}", state.Type, sent, ((IPEndPoint)state.GameSocket.RemoteEndPoint).Address, ((IPEndPoint)state.GameSocket.RemoteEndPoint).Port));
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
        
        private void WaitForServerData(ref LoginSocketState state)
        {
            Thread.Sleep(10);

            try
            {
                state.ServerSocket.BeginReceive(state.ServerBuffer, 0, state.ServerBuffer.Length, SocketFlags.None, OnServerDataReceived, state);
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

        private void WaitForData(ref LoginSocketState state)
        {
            Thread.Sleep(10);

            try
            {
                state.GameSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnGameDataReceived, state);
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

        private void OnServerDataReceived(IAsyncResult async)
        {
            LoginSocketState state = (LoginSocketState)async.AsyncState;

            if (state == null || state.ServerSocket == null)
                return;

            try
            {
                // receive data from the socket
                int received = state.ServerSocket.EndReceive(async);
                if (received == 0)
                {
                    // when EndReceive returns 0, it means the socket on the other end has been shut down.
                    return;
                }

                //var str = Encoding.UTF8.GetString(state.Buffer, 0, received);

                // take what we received, and append it to the received data buffer
                //state.ReceivedData.Append(Encoding.UTF8.GetString(state.Buffer, 0, received));
                //string receivedData = state.ReceivedData.ToString();

                SendToGame(ref state, state.ServerBuffer, received);
                //SendToServer(ref state, state.Buffer);


                // does what we received contain the \final\ delimiter?
                /*if (str.LastIndexOf(@"\final\") > -1)
                {
                    state.ReceivedData.Clear();


                    // lets split up the message based on the delimiter
                    string[] messages = receivedData.Split(new string[] { @"\final\" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < messages.Length; i++)
                    {
                        ParseMessage(ref state, messages[i]);
                    }
                }*/
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
            WaitForServerData(ref state);
        }

        private void OnGameDataReceived(IAsyncResult async)
        {
            LoginSocketState state = (LoginSocketState)async.AsyncState;

            if (state == null || state.GameSocket == null)
                return;

            try
            {
                // receive data from the socket
                int received = state.GameSocket.EndReceive(async);
                if (received == 0)
                {
                    // when EndReceive returns 0, it means the socket on the other end has been shut down.
                    return;
                }

                //var str = Encoding.UTF8.GetString(state.Buffer, 0, received);

                // take what we received, and append it to the received data buffer
                //state.ReceivedData.Append(Encoding.UTF8.GetString(state.Buffer, 0, received));
                //string receivedData = state.ReceivedData.ToString();

                SendToServer(ref state, state.Buffer, received);


                // does what we received contain the \final\ delimiter?
                /*if (str.LastIndexOf(@"\final\") > -1)
                {
                    state.ReceivedData.Clear();


                    // lets split up the message based on the delimiter
                    string[] messages = receivedData.Split(new string[] { @"\final\" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < messages.Length; i++)
                    {
                        ParseMessage(ref state, messages[i]);
                    }
                }*/
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
            WaitForData(ref state);
        }

        /*private void ParseMessage(ref LoginSocketState state, string message)
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

            //Log(Category, String.Format("[{0}] Received {1} query from: {2}:{3}", state.Type, query, ((IPEndPoint)state.Socket.RemoteEndPoint).Address, ((IPEndPoint)state.Socket.RemoteEndPoint).Port));
            
            switch (state.Type)
            {
                case LoginSocketState.SocketType.Client:
                    HandleClientManager(ref state, query, keyValues);
                    break;
                case LoginSocketState.SocketType.Search:
                    HandleSearchManager(ref state, query, keyValues);
                    break;
            }
        }*/

       /* private void HandleClientManager(ref LoginSocketState state, string query, Dictionary<string, string> keyValues)
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
                        //SendToClient(ref state, LoginServerMessages.SendProof(ref state, keyValues));
                        //state.StartKeepAlive(this);

                        break;

                    default:
                        break;
                }
            }
        }*/

        /*private void HandleSearchManager(ref LoginSocketState state, string query, Dictionary<string, string> keyValues)
        {
            if (state.State == 0)
            {
                //SendToServer();

                if (query.Equals("nicks", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SendToClient(ref state, LoginServerMessages.SendNicks(ref state, keyValues));
                }
                else if (query.Equals("check", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SendToClient(ref state, LoginServerMessages.SendCheck(ref state, keyValues));
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
        }*/


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
        
        public Socket ServerSocket = null;
        public Socket GameSocket = null;
        public byte[] ServerBuffer = new byte[8192];
        public byte[] Buffer = new byte[8192];

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
                    if (GameSocket != null)
                    {
                        GameSocket.Shutdown(SocketShutdown.Both);
                        GameSocket.Close();
                        GameSocket.Dispose();
                        GameSocket = null;
                    }

                    if (ServerSocket != null)
                    {
                        ServerSocket.Shutdown(SocketShutdown.Both);
                        ServerSocket.Close();
                        ServerSocket.Dispose();
                        ServerSocket = null;
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
