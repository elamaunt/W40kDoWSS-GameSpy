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
        readonly LinkedList<TcpClient> _clients = new LinkedList<TcpClient>();

        readonly WeakReference<TcpClient> _lastClient = new WeakReference<TcpClient>(null);

        public TcpClient LastClient
        {
            get
            {
                _lastClient.TryGetTarget(out TcpClient client);
                return client;
            }
        }

        TcpListenerEx _listener;
        CancellationTokenSource _acceptingTokenSource;
        CancellationTokenSource _connectionTokenSource;

        ExceptionHandler _exceptionHandlerDelegate;
        DataHandler _handlerDelegate;
        ZeroHandler _zeroHandlerDelegate;
        AcceptHandler _acceptDelegate;

        readonly byte[] _buffer = new byte[65536];

        public delegate void ZeroHandler(TcpPortHandler handler);
        public delegate void ExceptionHandler(Exception exception, bool send, int port);
        public delegate void AcceptHandler(TcpPortHandler handler, TcpClient client, CancellationToken token);
        public delegate void DataHandler(TcpPortHandler handler, TcpClient client, byte[] buffer, int count);

        readonly int _port;

        public TcpPortHandler(int port, DataHandler handlerDelegate, ExceptionHandler errorHandler = null, AcceptHandler acceptDelegate = null, ZeroHandler zeroHandler = null)
        {
            _port = port;
            _zeroHandlerDelegate = zeroHandler;
            _exceptionHandlerDelegate = errorHandler;
            _handlerDelegate = handlerDelegate;
            _acceptDelegate = acceptDelegate;
            _listener = new TcpListenerEx(IPAddress.Any, _port);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            _acceptingTokenSource = new CancellationTokenSource();
            _listener.Start();
            _listener.AcceptTcpClientAsync().ContinueWith(OnAccept, _acceptingTokenSource.Token);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            _acceptingTokenSource?.Cancel();
            _acceptingTokenSource = null;
            _connectionTokenSource?.Cancel();
            _connectionTokenSource = null;
            
            _listener.Stop();

            var set = _clients.ToArray();
            _clients.Clear();

            for (int i = 0; i < set.Length; i++)
                set[i].Dispose();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void OnAccept(Task<TcpClient> task)
        {
            try
            {
                if (task.IsFaulted)
                    throw task.Exception.GetInnerException();

                _connectionTokenSource?.Cancel();
                _connectionTokenSource = new CancellationTokenSource();

                var client = task.Result;
                _lastClient.SetTarget(client);

                client.NoDelay = true;
                client.SendTimeout = 30000;
                client.LingerState = new LingerOption(false, 0);

                _clients.AddLast(client);

                _acceptDelegate?.Invoke(this, client, _connectionTokenSource.Token);

                client.GetStream().ReadAsync(_buffer, 0, _buffer.Length, _connectionTokenSource.Token).ContinueWith(t => OnReceive(client, t));
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

        public bool SendUtf8(TcpClient client, string message)
        {
            return Send(client, message.ToUTF8Bytes());
        }

        public bool SendAskii(TcpClient client, string message)
        {
            return Send(client, message.ToAssciiBytes());
        }

        public bool Send(TcpClient client, byte[] bytes)
        {
            if (client == null)
                return false;

            try
            {
                var stream = client.GetStream();

                if (stream != null)
                {
                    stream.Write(bytes, 0, bytes.Length);
                    return true;
                }
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
                _exceptionHandlerDelegate?.Invoke(ex, true, _port);
            }

            return false;
        }

        void OnReceive(TcpClient client, Task<int> task)
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
                    _handlerDelegate(this, client, _buffer, count);
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
            }
            finally
            {
                try
                {
                    var source = _connectionTokenSource;

                    if (source != null)
                        client?.GetStream()?.ReadAsync(_buffer, 0, _buffer.Length, source.Token).ContinueWith(t => OnReceive(client, t));
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

        public void KillClient(TcpClient client)
        {
            if (_clients.Remove(client))
            {
                client.GetStream()?.Close(2000);
                client.Close();
                client.Dispose();
            }
        }
    }
}
