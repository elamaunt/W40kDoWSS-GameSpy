using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ThunderHawk.Core;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public class UdpPortHandler
    {
        Socket _socket;
        SocketAsyncEventArgs _socketReadEvent;
        byte[] _socketReceivedBuffer;

        ExceptionHandler _exceptionHandlerDelegate;
        DataHandler _handlerDelegate;

        public delegate void ExceptionHandler(Exception exception, bool send, int port);
        public delegate void DataHandler(UdpPortHandler handler, byte[] bytes, IPEndPoint endPoint);

        readonly int _port;
        public UdpPortHandler(int port, DataHandler handlerDelegate, ExceptionHandler errorHandler)
        {
            _port = port;
            _exceptionHandlerDelegate = errorHandler;
            _handlerDelegate = handlerDelegate;
        }

        public void Start()
        {
            var addresses = NetworkHelper.GetLocalIpAddresses();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                SendTimeout = 5000,
                ReceiveTimeout = 5000,
                SendBufferSize = 65535,
                ReceiveBufferSize = 65535,
                Blocking = false
            };

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);

            // Fixing connection reset bug
            const int SIO_UDP_CONNRESET = -1744830452;
            byte[] inValue = new byte[] { 0 };
            byte[] outValue = new byte[] { 0 };
            _socket.IOControl(SIO_UDP_CONNRESET, inValue, outValue);

            var address = IPAddress.Any; //addresses.FirstOrDefault() ?? IPAddress.Loopback;

            _socket.Bind(new IPEndPoint(address, _port));

            _socketReadEvent = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(address, 0)
            };

            _socketReceivedBuffer = new byte[65535];
            _socketReadEvent.SetBuffer(_socketReceivedBuffer, 0, _socketReceivedBuffer.Length);
            _socketReadEvent.Completed += OnDataReceived;


            if (!_socket.ReceiveFromAsync(_socketReadEvent))
                OnDataReceived(this, _socketReadEvent);
        }

        void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted)
                return;

            if (e.SocketError == SocketError.ConnectionReset)
            {
                Thread.Sleep(10);

                Logger.Warn($"=========== SWITCH SOCKET {_port} {e.SocketError}");

                e.SocketError = SocketError.Success;

                try
                {
                    if (!_socket.ReceiveFromAsync(_socketReadEvent))
                        OnDataReceived(this, _socketReadEvent);
                }
                catch (OperationCanceledException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (SocketException)
                {
                }

                return;
            }

            var bytes = new byte[e.BytesTransferred];
            Array.Copy(e.Buffer, 0, bytes, 0, e.BytesTransferred);

            _handlerDelegate(this, bytes, (IPEndPoint)e.RemoteEndPoint);
           
            Thread.Sleep(10);

            if (!_socket.ReceiveFromAsync(_socketReadEvent))
                OnDataReceived(this, _socketReadEvent);
        }

        public void Stop()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
        }

        public void Send(byte[] bytes, IPEndPoint point)
        {
            try
            {
                var sended = _socket?.SendTo(bytes, 0, bytes.Length, SocketFlags.None, point);

                if (sended != bytes.Length)
                    throw new Exception("Sended inconsystency data");
            }
            catch (OperationCanceledException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (SocketException)
            {
            }
            catch (Exception ex)
            {
                _exceptionHandlerDelegate(ex, true, _port);
            }
        }
    }
}
