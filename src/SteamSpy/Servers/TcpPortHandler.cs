using GSMasterServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class TcpPortHandler
    {
        readonly LinkedList<TcpClientNode> _clients = new LinkedList<TcpClientNode>();

        readonly WeakReference<TcpClientNode> _lastClient = new WeakReference<TcpClientNode>(null);

        public TcpClientNode LastClient
        {
            get
            {
                _lastClient.TryGetTarget(out TcpClientNode client);
                return client;
            }
        }

        TcpListenerEx _listener;
        CancellationTokenSource _tokenSource;

        ExceptionHandler _exceptionHandlerDelegate;
        DataHandler _handlerDelegate;
        ZeroHandler _zeroHandlerDelegate;
        AcceptHandler _acceptDelegate;

        public delegate void ZeroHandler(TcpPortHandler handler);
        public delegate void ExceptionHandler(Exception exception, bool send, int port);
        public delegate void AcceptHandler(TcpPortHandler handler, TcpClientNode node, CancellationToken token);
        public delegate void DataHandler(TcpPortHandler handler, TcpClientNode node, byte[] buffer, int count);

        readonly int _port;

        readonly object RECEIVE_LOCKER = new object();

        public TcpPortHandler(int port, DataHandler handlerDelegate, ExceptionHandler errorHandler = null, AcceptHandler acceptDelegate = null, ZeroHandler zeroHandler = null)
        {
            _port = port;
            _zeroHandlerDelegate = zeroHandler;
            _exceptionHandlerDelegate = errorHandler;
            _handlerDelegate = handlerDelegate;
            _acceptDelegate = acceptDelegate;
            _listener = new TcpListenerEx(IPAddress.Any, _port);

            _listener.ExclusiveAddressUse = true;
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _listener.Start();
            _listener.AcceptTcpClientAsync().ContinueWith(OnAccept, _tokenSource.Token);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            _tokenSource?.Cancel();
            _tokenSource = null;
            
            _listener.Stop();

            var set = _clients.ToArray();
            _clients.Clear();

            for (int i = 0; i < set.Length; i++)
                set[i].Client.Dispose();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void OnAccept(Task<TcpClient> task)
        {
            try
            {
                if (task.IsFaulted)
                    throw task.Exception.GetInnerException();

                var client = task.Result;
                var node = new TcpClientNode(client);
                _lastClient.SetTarget(node);

                client.NoDelay = true;
                client.SendTimeout = 30000;

                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);


                _clients.AddLast(node);

                _acceptDelegate?.Invoke(this, node, _tokenSource.Token);

                client.GetStream().ReadAsync(node.Buffer, 0, node.Buffer.Length, _tokenSource.Token).ContinueWith(t => OnReceive(node, t));
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
                _exceptionHandlerDelegate?.Invoke(ex, false, _port);
                Logger.Error(ex);
            }
            finally
            {
                if (_listener.Active)
                    _listener.AcceptTcpClientAsync().ContinueWith(OnAccept);
            }
        }

        public bool SendUtf8(string message)
        {
            return Send(LastClient, message.ToUTF8Bytes());
        }

        public bool SendAskii(string message)
        {
            return Send(LastClient, message.ToAssciiBytes());
        }

        public bool Send(byte[] bytes)
        {
            return Send(LastClient, bytes);
        }

        public bool SendUtf8(TcpClientNode node, string message)
        {
            return Send(node, message.ToUTF8Bytes());
        }

        public bool SendAskii(TcpClientNode node, string message)
        {
            return Send(node, message.ToAssciiBytes());
        }

        public bool Send(TcpClientNode node, byte[] bytes)
        {
            if (node == null)
                return false;

            try
            {
                var stream = node.Client.GetStream();

                if (stream != null)
                {
                    stream.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.Info(_port + " send : " + ex);
                KillClient(node);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Info(_port + " send : " + ex);
                KillClient(node);
            }
            catch (SocketException ex)
            {
                Logger.Info(_port + " send : " + ex);
                KillClient(node);
            }
            catch (Exception ex)
            {
                Logger.Info(_port + " send : " + ex);
                KillClient(node);
                _exceptionHandlerDelegate?.Invoke(ex, true, _port);
            }

            return false;
        }

        void OnReceive(TcpClientNode node, Task<int> task)
        {
            try
            {
                if (task.IsCanceled)
                    return;

                if (task.IsFaulted)
                    throw task.Exception.GetInnerException();

                var count = task.Result;

                if (count == 0)
                    _zeroHandlerDelegate?.Invoke(this);
                else
                    _handlerDelegate(this, node, node.Buffer, count);
            }
            catch (OperationCanceledException)
            {
                KillClient(node);
            }
            catch (InvalidOperationException)
            {
                KillClient(node);
            }
            catch (SocketException)
            {
                KillClient(node);
            }
            catch (Exception ex)
            {
                KillClient(node);
                _exceptionHandlerDelegate?.Invoke(ex, false, _port);
            }
            finally
            {
                try
                {
                    if (node.Client.Connected)
                    {
                        var source = _tokenSource;

                        if (source != null)
                            node.Client.GetStream()?.ReadAsync(node.Buffer, 0, node.Buffer.Length, source.Token).ContinueWith(t => OnReceive(node, t));
                    }
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Info(_port + " recv : " + ex);
                    KillClient(node);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Info(_port + " recv : " + ex);
                    KillClient(node);
                }
                catch (SocketException ex)
                {
                    Logger.Info(_port + " recv : " + ex);
                    KillClient(node);
                }
                catch (Exception ex)
                {
                    Logger.Info(_port + " recv : " + ex);
                    KillClient(node);
                    _exceptionHandlerDelegate?.Invoke(ex, false, _port);
                }
            }
        }

        /// <summary>
        /// Wrapper around TcpListener that exposes the Active property
        /// </summary>
        public class TcpListenerEx : TcpListener
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener"/> class with the specified local endpoint.
            /// </summary>
            /// <param name="localEP">An <see cref="T:System.Net.IPEndPoint"/> that represents the local endpoint to which to bind the listener <see cref="T:System.Net.Sockets.Socket"/>. </param><exception cref="T:System.ArgumentNullException"><paramref name="localEP"/> is null. </exception>
            public TcpListenerEx(IPEndPoint localEP) 
                : base(localEP)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener"/> class that listens for incoming connection attempts on the specified local IP address and port number.
            /// </summary>
            /// <param name="localaddr">An <see cref="T:System.Net.IPAddress"/> that represents the local IP address. </param><param name="port">The port on which to listen for incoming connection attempts. </param><exception cref="T:System.ArgumentNullException"><paramref name="localaddr"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="F:System.Net.IPEndPoint.MaxPort"/>. </exception>
            public TcpListenerEx(IPAddress localaddr, int port)
                : base(localaddr, port)
            {
            }

            public new bool Active
            {
                get { return base.Active; }
            }
        }

        public void KillClient(TcpClientNode node)
        {
            if (_clients.Remove(node))
            {
                try
                {
                    node.Client.GetStream()?.Close(2000);
                    //node.Client.Close();
                    //node.Client.Dispose();
                }
                catch (Exception)
                {

                }
            }

            Logger.Info($"{_port} CLIENTS NOW {_clients.Count}");
        }

        public class TcpClientNode
        {
            public readonly TcpClient Client;
            public readonly byte[] Buffer = new byte[65536];

            public IPEndPoint RemoteEndPoint => (IPEndPoint)Client.Client?.RemoteEndPoint;

            public TcpClientNode(TcpClient client)
            {
                Client = client;
            }
        }
    }
}
