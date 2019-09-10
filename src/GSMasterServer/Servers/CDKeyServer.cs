using GSMasterServer.Data;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSMasterServer.Services;
using IrcNet.Tools;

namespace GSMasterServer.Servers
{
    internal class CDKeyServer
    {
        Thread _thread;

        private const int BufferSize = 8192;
        private Socket _socket;
        private SocketAsyncEventArgs _socketReadEvent;
        private byte[] _socketReceivedBuffer;

        private readonly Regex _dataPattern = new Regex(@"^\\auth\\\\pid\\1059\\ch\\[a-zA-z0-9]{8,10}\\resp\\(?<Challenge>[a-zA-z0-9]{72})\\ip\\\d+\\skey\\(?<Key>\d+)(\\reqproof\\[01]\\)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        private const string _dataResponse = @"\uok\\cd\{0}\skey\{1}";

        public CDKeyServer(IPAddress listen, ushort port)
        {
            _thread = new Thread(StartServer)
            {
                Name = "CD Key Thread"
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

        ~CDKeyServer()
        {
            Dispose(false);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Logger.Info("Starting CD Key Server");

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
                var ex = new Exception($"Unable to bind CD Key Server to {info.Address}:{info.Port}", e);
                Logger.Error(ex);
                return;
            }

            WaitForData();
        }

        private void WaitForData()
        {
            Thread.Sleep(10);

            try
            {
                if (!_socket.ReceiveFromAsync(_socketReadEvent))
                    OnDataReceived(this, _socketReadEvent);
            }
            catch (SocketException e)
            {
                var ex = new Exception("Error receiving data", e);
                Logger.Error(ex);
            }
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                string receivedData = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                string decrypted = Xor(receivedData);

                // known messages
                // \ka\ = keep alive from the game server every 20s, we don't care about this
                // \auth\ ... = authenticate cd key, this is what we care about
                // \disc\ ... = disconnect cd key, because there's checks if the cd key is in use, which we don't care about really, but we could if we wanted to

                // \ka\ is a keep alive from the game server, it's useless :p
                if (decrypted != @"\ka\")
                {
                    Match m = _dataPattern.Match(decrypted);

                    if (m.Success)
                    {
                        Logger.Trace($"Received request from: {((IPEndPoint)e.RemoteEndPoint).Address}:{((IPEndPoint)e.RemoteEndPoint).Port}");
                        string reply = String.Format(_dataResponse, m.Groups["Challenge"].Value.Substring(0, 32), m.Groups["Key"].Value);

                        byte[] response = Encoding.UTF8.GetBytes(Xor(reply));
                        _socket.SendTo(response, remote);
                    }
                }
            }
            catch (Exception)
            {
            }

            WaitForData();
        }

        private static string Xor(string s)
        {
            const string gamespy = "gamespy";
            int length = s.Length;
            char[] data = s.ToCharArray();
            int index = 0;

            for (int i = 0; length > 0; length--)
            {
                if (i >= gamespy.Length)
                    i = 0;

                data[index++] ^= gamespy[i++];
            }

            return new String(data);
        }
    }
}
