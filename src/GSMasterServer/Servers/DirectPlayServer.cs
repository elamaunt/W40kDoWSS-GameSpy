using GSMasterServer.Data;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class DirectPlayServer : Server
    {
        private const string Category = "DirectPlay";

        Thread _thread;

        private const int BufferSize = 8192;
        private Socket _socket;
        private SocketAsyncEventArgs _socketReadEvent;
        private byte[] _socketReceivedBuffer;

        public DirectPlayServer(IPAddress listen, ushort port)
        {
            _thread = new Thread(StartServer)
            {
                Name = "DirectPlay Thread"
            };

            _thread.Start(new AddressInfo()
            {
                Address = listen,
                Port = port
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
                    if (_socket != null)
                    {
                        _socket.Close();
                        _socket.Dispose();
                        _socket = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        ~DirectPlayServer()
        {
            Dispose(false);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Log(Category, "Starting DirectPlay Server");

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = BufferSize,
                    ReceiveBufferSize = BufferSize
                };

                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _socket.Bind(new IPEndPoint(info.Address, info.Port));

                _socketReadEvent = new SocketAsyncEventArgs()
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
                };
                _socketReceivedBuffer = new byte[BufferSize];
                _socketReadEvent.SetBuffer(_socketReceivedBuffer, 0, BufferSize);
                _socketReadEvent.Completed += OnDataReceived;
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind CD Key Server to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            WaitForData();
        }

        private void WaitForData()
        {
            Thread.Sleep(10);

            try
            {
                _socket.ReceiveFromAsync(_socketReadEvent);
            }
            catch (SocketException e)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, e.ToString());
                return;
            }
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                string receivedData = Encoding.ASCII.GetString(e.Buffer, e.Offset, e.BytesTransferred);

            }
            catch (Exception)
            {
            }

            WaitForData();
        }
    }
}
