using GSMasterServer.Utils;
using System;
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
        TcpClient _client;
        TcpListenerEx _listener;
        CancellationTokenSource _acceptingTokenSource;
        CancellationTokenSource _connectionTokenSource;

        ExceptionHandler _exceptionHandlerDelegate;
        DataHandler _handlerDelegate;
        ZeroHandler _zeroHandlerDelegate;
        AcceptHandler _acceptDelegate;

        readonly byte[] _buffer = new byte[65536];

        public delegate void ZeroHandler(TcpPortHandler handler);
        public delegate void ExceptionHandler(Exception exception, bool send);
        public delegate void AcceptHandler(TcpPortHandler handler, TcpClient client, CancellationToken token);
        public delegate void DataHandler(TcpPortHandler handler, byte[] buffer, int count);

        readonly int _port;

        public IPEndPoint RemoteEndPoint => (IPEndPoint)_client?.Client?.RemoteEndPoint;

        public TcpPortHandler(int port, DataHandler handlerDelegate, ExceptionHandler errorHandler, AcceptHandler acceptDelegate = null, ZeroHandler zeroHandler = null)
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
            _client?.Dispose();
            _client = null;
            _listener.Stop();
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
                _client?.Close();
                _client?.Dispose();
                _client = task.Result;
                _client.NoDelay = true;
                _client.SendTimeout = 30000;
                _client.LingerState = new LingerOption(false, 0);

                _acceptDelegate?.Invoke(this, _client, _connectionTokenSource.Token);
                _client.GetStream().ReadAsync(_buffer, 0, _buffer.Length, _connectionTokenSource.Token).ContinueWith(OnReceive);
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
                _exceptionHandlerDelegate?.Invoke(ex, false);
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
            return Send(message.ToUTF8Bytes());
        }

        public bool SendAskii(string message)
        {
            return Send(message.ToAssciiBytes());
        }

        public bool Send(byte[] bytes)
        {
            try
            {
                var stream = _client?.GetStream();

                if (stream != null)
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
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
                _exceptionHandlerDelegate?.Invoke(ex, true);
            }

            return false;
        }

        void OnReceive(Task<int> task)
        {
            try
            {
                if (task.IsFaulted)
                    throw task.Exception.GetInnerException();

                var count = task.Result;

                if (count == 0)
                    _zeroHandlerDelegate?.Invoke(this);
                else
                    _handlerDelegate(this, _buffer, count);
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
                _exceptionHandlerDelegate?.Invoke(ex, false);
            }
            finally
            {
                try
                {
                    _client?.GetStream()?.ReadAsync(_buffer, 0, _buffer.Length, _connectionTokenSource.Token).ContinueWith(OnReceive);
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
                    _exceptionHandlerDelegate?.Invoke(ex, false);
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

        public void KillCurrentClient()
        {
            _client?.GetStream()?.Flush();
            _client?.GetStream()?.Dispose();
            _client = null;
        }
    }
}
