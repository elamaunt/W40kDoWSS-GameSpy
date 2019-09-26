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
        TcpListener _listener;
        CancellationTokenSource _tokenSource;

        DataHandler _handlerDelegate;
        AcceptHandler _acceptDelegate;

        readonly byte[] _buffer = new byte[65536];

        public delegate void AcceptHandler(TcpPortHandler handler, TcpClient client, CancellationToken token);
        public delegate void DataHandler(TcpPortHandler handler, byte[] buffer, int count);
        public TcpPortHandler(int port, DataHandler handlerDelegate, AcceptHandler acceptDelegate = null)
        {
            _handlerDelegate = handlerDelegate;
            _acceptDelegate = acceptDelegate;

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _listener.AcceptTcpClientAsync().ContinueWith(OnAccept);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void OnAccept(Task<TcpClient> task)
        {
            try
            {
                _tokenSource?.Cancel();
                _tokenSource = new CancellationTokenSource();
                _client?.Close();
                _client?.Dispose();
                _client = task.Result;
                _client.NoDelay = true;
                _client.SendTimeout = 30000;
                _client.LingerState = new LingerOption(false, 0);
                _client.ExclusiveAddressUse = true;

                _acceptDelegate?.Invoke(this, _client, _tokenSource.Token);
                _client.GetStream().ReadAsync(_buffer, 0, _buffer.Length, _tokenSource.Token).ContinueWith(OnReceive);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                _listener.AcceptTcpClientAsync().ContinueWith(OnAccept);
            }
        }

        public void Send(byte[] bytes, IPEndPoint point)
        {
            try
            {
                _client.GetStream().Write(bytes, 0, bytes.Length);
            }
            catch (InvalidOperationException)
            {

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        void OnReceive(Task<int> task)
        {
            try
            {
                _handlerDelegate(this, _buffer, task.Result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                try
                {
                    _client.GetStream().ReadAsync(_buffer, 0, _buffer.Length, _tokenSource.Token).ContinueWith(OnReceive);
                }
                catch(InvalidOperationException)
                {

                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }
    }
}
