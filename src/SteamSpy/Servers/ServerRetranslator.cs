﻿using GSMasterServer.Data;
using Steamworks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class ServerRetranslator : Server
    {
        public const string Category = "ServerRetranslator";
        const int BufferSize = 65535;

        CSteamID _userId;

        Socket _socket;
        SocketAsyncEventArgs _socketReadEvent;
        byte[] _socketReceivedBuffer;

        static readonly IPEndPoint GameEndPoint = new IPEndPoint(IPAddress.Loopback, 6112);

        public ushort Port { get; private set; }
        public IPEndPoint LocalPoint { get; set; }

        public ServerRetranslator(CSteamID userId, IPEndPoint point = null)
        {
            _userId = userId;
            LocalPoint = point;

            GeoIP.Initialize(Log, Category);
            StartServer();
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
                        _socketReadEvent.Completed -= OnDataReceived;
                        _socketReadEvent.Dispose();
                        _socketReadEvent = null;

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

        public void Clear()
        {
            LocalPoint = null;
        }

        ~ServerRetranslator()
        {
            Dispose(false);
        }

        private void StartServer()
        {
            Log(Category, "Starting Server Retranslator");

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
                _socket.Bind(new IPEndPoint(IPAddress.Any, 0));

                Port = (ushort)((IPEndPoint)_socket.LocalEndPoint).Port;

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
                LogError(Category, e.ToString());
                return;
            }

            WaitForData();
        }

        private void WaitForData()
        {
            Thread.Sleep(10);
            GC.Collect();

            try
            {
                if (_socket == null)
                    Console.WriteLine("WAIT SOCKET NULL");

                if (!_socket.ReceiveFromAsync(_socketReadEvent))
                    OnDataReceived(this, _socketReadEvent);
            }
            catch(NullReferenceException ex)
            {

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
                LocalPoint = e.RemoteEndPoint as IPEndPoint;

                byte[] receivedBytes = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, receivedBytes, 0, e.BytesTransferred);

                var str = Encoding.UTF8.GetString(receivedBytes);

                // there by a bunch of different message formats...
                Log(Category, ">> "+str);

                //Console.WriteLine("SendTo "+ _userId.m_SteamID+" "+ e.BytesTransferred);
                // IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;
                SteamNetworking.SendP2PPacket(_userId, e.Buffer, (uint)e.BytesTransferred, EP2PSend.k_EP2PSendUnreliableNoDelay);
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }

            WaitForData();
        }

        public void Send(byte[] buffer, uint size)
        {
            try
            {
                var str = Encoding.UTF8.GetString(buffer, 0, (int)size);

                // there by a bunch of different message formats...
                Log(Category,"<= "+ str);

                // Console.WriteLine("ReceivedFromUser "+ _userId.m_SteamID+" "+ size);

                if (_socket == null)
                    Console.WriteLine("RECEIVE SOCKET NULL");

                _socket?.SendTo(buffer, (int)size, SocketFlags.None, LocalPoint ?? GameEndPoint);
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }
        }
    }
}
