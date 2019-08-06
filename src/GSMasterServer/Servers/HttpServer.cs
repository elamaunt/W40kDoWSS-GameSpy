using GSMasterServer.Data;
using GSMasterServer.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace GSMasterServer.Servers
{
    internal class HttpServer 
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private const string Category = "Http";
        
        Thread _thread;
        Socket _newPeerAceptingsocket;
        private DateTime _lastStatsUpdate = DateTime.Now;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);

        public HttpResponse StatsResponce { get; set; }

        public HttpServer(IPAddress listen, ushort port)
        {
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

            logger.Info("Starting Http Reporting");

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
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _newPeerAceptingsocket.Bind(new IPEndPoint(info.Address, info.Port));
                _newPeerAceptingsocket.Listen(10);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Unable to bind Http Server to {info.Address}:{info.Port}");
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
                logger.Error(e, $"Error accepting client. SocketErrorCode: {e.SocketErrorCode}");
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
                    logger.Error(e, $"Error receiving data. SocketErrorCode: {e.SocketErrorCode}");
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
                
                logger.Info("Data received: " + Encoding.UTF8.GetString(state.Buffer, 0, received).Replace('\n', ' ').Replace('\r', ' '));


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
                        if (StatsResponce == null || (DateTime.Now - _lastStatsUpdate).TotalMinutes > 5)
                            StatsResponce = BuildTop10StatsResponce();

                        HttpHelper.WriteResponse(ms, StatsResponce);
                        goto END;
                    }
                    
                    if (request.Url.StartsWith("/all"))
                    {
                        HttpHelper.WriteResponse(ms, BuildAllStatsResponce());
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
                        logger.Error(e, $"Error receiving data. SocketErrorCode: {e.SocketErrorCode}");
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error receiving data");
            }

            // and we wait for more data...
            CONTINUE: WaitForData(ref state);
        }

        private HttpResponse BuildAllStatsResponce()
        {
            XElement ol;

            var xDocument = new XDocument(
                new XDocumentType("html", null, null, null),
                new XElement("html",
                    new XElement("head", new XElement("b", "Ladder Top 10")),
                    new XElement("body",
                        new XElement("p", "Updates every 5 minutes"),
                        ol = new XElement("ol")


                    )
                )
            );

            var builder = new StringBuilder();

            foreach (var item in Database.UsersDBInstance.LoadAllStats())
            {
                builder
                 .Append(item.Score1v1)
                 .Append("   -   ")
                 .Append(item.Name)
                 .Append("   |   ")
                 .Append(GetRaceName(item.FavouriteRace))
                 .Append("   |   ")
                 .Append(item.SteamId);

                ol.Add(new XElement("li", builder.ToString()));
                builder.Clear();
            }

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "\t"
            };

            using (var ms = new MemoryStream())
            using (var writer = XmlWriter.Create(ms, settings))
            {
                xDocument.WriteTo(writer);
                writer.Flush();
                return new HttpResponse()
                {
                    ReasonPhrase = "Ok",
                    StatusCode = "200",
                    Content = ms.ToArray()
                };
            }
        }


        private HttpResponse BuildTop10StatsResponce()
        {
            XElement ol;

            var xDocument = new XDocument(
                new XDocumentType("html", null, null, null),
                new XElement("html",
                    new XElement("head", new XElement("b", "Ladder Top 10")),
                    new XElement("body",
                        new XElement("p", "Updates every 5 minutes"),
                        ol = new XElement("ol")


                    )
                )
            );

            var builder = new StringBuilder();

            foreach (var item in Database.UsersDBInstance.Load1v1Top10())
            {
                builder
                .Append(item.Score1v1)
                 .Append("   -   ")
                .Append(item.WinsCount)
                .Append("/")
                .Append(item.GamesCount)
                 .Append("   -   ")
                .Append(((int)(item.WinRate * 100f)) + "%")
                .Append("   -   ")
                .Append(item.Name)
                .Append("   |   ")
                .Append(GetRaceName(item.FavouriteRace));
                
                ol.Add(new XElement("li", builder.ToString()));
                builder.Clear();
            }

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "\t"
            };

            using (var ms = new MemoryStream())
            using (var writer = XmlWriter.Create(ms, settings))
            {
                xDocument.WriteTo(writer);
                writer.Flush();
                return new HttpResponse()
                {
                    ReasonPhrase = "Ok",
                    StatusCode = "200",
                    Content = ms.ToArray()
                };
            }
        }

        private string GetRaceName(Race favouriteRace)
        {
            switch (favouriteRace)
            {
                case Race.space_marine_race: return "Space Marines";
                case Race.chaos_marine_race: return "Chaos Space Marines";
                case Race.ork_race: return "Orks";
                case Race.eldar_race: return "Eldar";
                case Race.guard_race: return "Imperial Guard";
                case Race.necron_race: return "Necrons";
                case Race.tau_race: return "Tau";
                case Race.dark_eldar_race: return "Dark Eldar";
                case Race.sisters_race: return "Sisters of Battle";
                default: return "Unknown";
            }
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
