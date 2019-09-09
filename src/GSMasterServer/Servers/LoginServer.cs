extern alias reality;

using GSMasterServer.Data;
using GSMasterServer.Utils;
using reality::Reality.Net.Extensions;
using reality::Reality.Net.GameSpy.Servers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GSMasterServer.Services;
using NLog.Fluent;

namespace GSMasterServer.Servers
{
    internal class LoginServer
    {

        Thread _clientManagerThread;
        Thread _searchManagerThread;

        private static Socket _clientManagerSocket;
        private static Socket _searchManagerSocket;

        private readonly ManualResetEvent _clientManagerReset = new ManualResetEvent(false);
        private readonly ManualResetEvent _searchManagerReset = new ManualResetEvent(false);

        public static readonly ConcurrentDictionary<long, LoginSocketState> Users = new ConcurrentDictionary<long, LoginSocketState>();

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

            Logger.Info("Starting Login Server ClientManager");

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
                Logger.Error(e, $"Unable to bind Login Server ClientManager to {info.Address}:{info.Port}");
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

            Logger.Info("Starting Login Server SearchManager");

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
                Logger.Error(e, $"Unable to bind Login Server SearchManager to {info.Address}:{info.Port}");
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

                Logger.Trace( $"[{state.Type}] New Client: { ((IPEndPoint)state.Socket.RemoteEndPoint).Address}:{((IPEndPoint)state.Socket.RemoteEndPoint).Port}");

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
                Logger.Error(e, $"Error accepting client. SocketErrorCode: {e.SocketErrorCode}");
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

            Logger.Trace($"RESP: {DataFunctions.BytesToString(data)}");

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
                    Logger.Error(e, $"Error sending data. SocketErrorCode: {e.SocketErrorCode}");
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
                Logger.Trace($"[{state.Type}] Sent {sent} byte response to: " +
                            $"{((IPEndPoint)state.Socket.RemoteEndPoint).Address}:{((IPEndPoint)state.Socket.RemoteEndPoint).Port}");
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
                        Logger.Error(e, $"Error sending data. SocketErrorCode: {e.SocketErrorCode}");
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
                    Logger.Error(e,$"Error receiving data. SocketErrorCode: {e.SocketErrorCode}");
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
                        Logger.Error(e,$"Error receiving data. SocketErrorCode: {e.SocketErrorCode}");
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e,$"Error receiving data");
            }

            // and we wait for more data...
            CONTINUE: WaitForData(ref state);
        }

        private void ParseMessage(ref LoginSocketState state, string message)
        {
            string query;
            Logger.Trace($"MESSAGE: {message}");

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

            Logger.Trace($"[{state.Type}] Received {query} query from: " +
                        $"{((IPEndPoint)state.Socket.RemoteEndPoint).Address}:{((IPEndPoint)state.Socket.RemoteEndPoint).Port}");

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
                        Users[state.ProfileId] = state;
                        state.StartKeepAlive(this);

                        break;

                    case "newuser":
                        SendToClient(ref state, LoginServerMessages.NewUser(ref state, keyValues));
                        break;

                    case "addbuddy":
                        // \\addbuddy\\\\sesskey\\51437\\newprofileid\\1\\reason\\just be my friend

                        var newFriendId = long.Parse(keyValues["newprofileid"]);

                        if (Database.MainDBInstance.AddFriend(state.ProfileId, newFriendId))
                        {
                            //var signature = (state.ProfileId.ToString() + newFriendId.ToString()).ToMD5();
                            // "\\bm\\%d\\f\\%d\\msg\\%s|signed|%s",GPI_BM_REQUEST,profileid,reason,signature);
                            //SendToClient(ref state, $@"\bm\2\f\{newFriendId}\msg\{keyValues["reason"]}|signed|{signature}".ToAssciiBytes());

                            if (Users.TryGetValue(newFriendId, out LoginSocketState friendState))
                            {
                                SendToClient(ref friendState, $@"\bm\100\f\{state.ProfileId}\msg\|s|{state.Status}{state.GetLSParameter()}|ss|{state.StatusString}\final\".ToAssciiBytes());
                                SendToClient(ref state, $@"\bm\100\f\{newFriendId}\msg\|s|{friendState.Status}{friendState.GetLSParameter()}|ss|{friendState.StatusString}\final\".ToAssciiBytes());
                            }
                            else
                            {
                                SendToClient(ref state, $@"\bm\100\f\{newFriendId}\msg\|s|0|ss|Offline\final\".ToAssciiBytes());
                            }
                        }
                        break;
                    case "delbuddy":
                        // \delbuddy\\sesskey\51437\delprofileid\1
                        Database.MainDBInstance.RemoveFriend(state.ProfileId, long.Parse(keyValues["delprofileid"]));
                        break;
                    case "getprofile":
                        SendToClient(ref state, LoginServerMessages.SendProfile(ref state, keyValues));
                        break;

                    case "updatepro":
                        //LoginServerMessages.UpdateProfile(ref state, keyValues);
                        break;

                    case "status":

                        var remoteEndPoint = ((IPEndPoint)state.Socket.RemoteEndPoint);

                        var profile = Database.MainDBInstance.GetProfileById(state.ProfileId);

                        if (profile == null)
                            return;

                        var friends = profile.Friends ?? new List<long>();
                        
                        SendToClient(ref state, $@"\bdy\{friends.Count + 1}\list\{string.Join(",",friends.Concat(new long[] { profile.Id }))}\final\".ToAssciiBytes());

                       // var loopbackIp = (uint)IPAddress.NetworkToHostOrder((int)IPAddress.Loopback.Address);
                       // var loopbackIp = ReverseEndian32((uint)IPAddress.NetworkToHostOrder((int)IPAddress.Parse("192.168.159.128").Address));
                       // var port = ReverseEndian16(27900);

                        state.Status = keyValues.GetOrDefault("status") ?? state.Status ?? "0";
                        state.StatusString = keyValues.GetOrDefault("statstring") ?? state.StatusString ?? "Offline";
                        state.LocString = keyValues.GetOrDefault("locstring") ?? state.LocString ?? "-1";

                        //SendToClient(ref state, $@"\bm\100\f\{profile.Id}\msg\|s|{1}|ss|DXP{"|ls|"}{-1}|ip|{loopbackIp}|p|{port}|qm|{0}\final\".ToAssciiBytes());

                        var statusResult = $@"\bm\100\f\{profile.Id}\msg\|s|{state.Status}{state.GetLSParameter()}|ss|{state.StatusString}\final\".ToAssciiBytes();
                        SendToClient(ref state, statusResult);

                        for (int i = 0; i < friends.Count; i++)
                        {
                            var friendId = friends[i];
                            
                            if (Users.TryGetValue(friendId, out LoginSocketState friendState))
                            {
                                SendToClient(ref friendState, statusResult);
                                //SendToClient(ref state, $@"\bm\100\f\{friendId}\msg\|s|{1}|ss|DXP|ls|-1\final\".ToAssciiBytes());
                                SendToClient(ref state, $@"\bm\100\f\{friendId}\msg\|s|{friendState.Status}{friendState.GetLSParameter()}|ss|{friendState.StatusString}\final\".ToAssciiBytes());
                            }
                            else
                            {
                                SendToClient(ref state, $@"\bm\100\f\{friendId}\msg\|s|0|ss|Offline\final\".ToAssciiBytes());
                            }
                        }

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
                        Users.TryRemove(state.ProfileId, out LoginSocketState removingState);
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
        
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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

        public long ProfileId;
        public string ServerChallenge;
        public string ClientChallenge;
        public ulong SteamId;
        public string Name;
        public string Email;
        public string PasswordEncrypted;

        private Timer _keepAliveTimer;

        public string Status;
        public string StatusString;
        public string LocString;
        
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

                Logger.Trace("sending keep alive");
                if (!server.SendToClient(ref state, LoginServerMessages.SendKeepAlive()))
                {
                    Dispose();
                    return;
                }

                // every 2nd keep alive request, we send an additional heartbeat
                if (HeartbeatState % 2 == 0)
                {
                    Logger.Trace("sending heartbeat");
                    if (!server.SendToClient(ref state, LoginServerMessages.SendHeartbeat()))
                    {
                        Dispose();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error running keep alive");
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

                LoginServer.Users.TryRemove(ProfileId, out LoginSocketState removingState);

                // yeah yeah, this is terrible, but it stops a memory leak :|
                GC.Collect();
            }
            catch (Exception)
            {
            }
        }

        public string GetLSParameter()
        {
            if (LocString.IsNullOrWhiteSpace() || LocString == "0")
                return string.Empty;
            return "|ls|" + LocString;

        }

        ~LoginSocketState()
        {
            Dispose(false);
        }
    }
}
