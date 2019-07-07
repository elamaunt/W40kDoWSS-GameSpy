using GSMasterServer.Data;
using GSMasterServer.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSMasterServer.Servers
{
    internal class HttpServer : Server
    {
        private const string Category = "Http";
        
        Thread _thread;
        Socket _newPeerAceptingsocket;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);

        public HttpServer(IPAddress listen, ushort port)
        {
            GeoIP.Initialize(Log, Category);
            
            _thread = new Thread(StartServer)
            {
                Name = "Http Socket Thread"
            };

            _thread.Start(new AddressInfo()
            {
                Address = listen,
                Port = port
            });
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Log(Category, "Starting Http Reporting");

            try
            {
                _newPeerAceptingsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = 8192,
                    ReceiveBufferSize = 8192,
                    Blocking = false
                };

                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _newPeerAceptingsocket.Bind(new IPEndPoint(info.Address, info.Port));
                _newPeerAceptingsocket.Listen(10);
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Http Server to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            while (true)
            {
                _reset.Reset();

                HttpSocketState state = new HttpSocketState()
                {
                    Socket = _newPeerAceptingsocket
                };

                _newPeerAceptingsocket.BeginAccept(AcceptCallback, state);
                _reset.WaitOne();
            }
        }

        void AcceptCallback(IAsyncResult ar)
        {
            HttpSocketState state = (HttpSocketState)ar.AsyncState;

            try
            {
                Socket client = state.Socket.EndAccept(ar);

                Thread.Sleep(1);

                _reset.Set();

                state.Socket = client;
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

        private void WaitForData(ref HttpSocketState state)
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
            HttpSocketState state = (HttpSocketState)async.AsyncState;

            if (state == null || state.Socket == null)
                return;

            try
            {
                int received = state.Socket.EndReceive(async);
                if (received == 0)
                    return;
                
                HttpRequest request;

                using (var ms = new MemoryStream(state.Buffer, 0, received))
                    request = HttpHelper.GetRequest(ms);
                
                Log(Category, Encoding.UTF8.GetString(state.Buffer, 0, received).Replace('\n', ' ').Replace('\r', ' '));


                using (var ms = new MemoryStream(state.Buffer))
                {
                    if (request.Url.EndsWith("news.txt"))
                    {
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.File("Resources/Files/Russiandow_news.txt", Encoding.Unicode));
                        goto END;
                    }

                    if (request.Url.StartsWith("/motd/motd"))
                    {
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.File("Resources/Files/Russiandow_news.txt", Encoding.Unicode));
                        goto END;
                    }

                    if (request.Url.StartsWith("/motd/vercheck"))
                    {
                        //HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(@"\newver\1\newvername\1.4\dlurl\http://127.0.0.1/NewPatchHere.exe"));
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(@"\newver\0"));
                        goto END;
                    }

                    if (request.Url.EndsWith("LobbyRooms.lua"))
                    {
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.File("Resources/Files/LobbyRooms.lua", Encoding.ASCII));
                        goto END;
                    }

                    if (request.Url.EndsWith("AutomatchDefaultsSS.lua") || request.Url.EndsWith("AutomatchDefaultsDXP2Fixed.lua"))
                    {
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.File("Resources/Files/AutomatchDefaults.lua", Encoding.ASCII));
                        goto END;
                    }

                    if (request.Url.EndsWith("homepage.php.htm"))
                    {
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.File("Resources/Pages/ComingSoon.html"));
                        goto END;
                    }
                    
                    HttpHelper.WriteResponse(ms, HttpResponceBuilder.NotFound());
                    
                    END: // Завершение отправки
                    var length = (int)ms.Position;
                    ms.Position = 0;

                    state.Socket.Send(state.Buffer, 0, length, SocketFlags.None);
                    state.Dispose();
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

        internal class HttpSocketState : IDisposable
        {
            public Socket Socket = null;
            public byte[] Buffer = new byte[8192];
            
            private Timer _keepAliveTimer;
            
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

            ~HttpSocketState()
            {
                Dispose(false);
            }
        }
    }
}
