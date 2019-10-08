using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ThunderHawk
{
    public class UdpPortHandler
    {
        UdpClient _client;

        ExceptionHandler _exceptionHandlerDelegate;
        DataHandler _handlerDelegate;

        public delegate void ExceptionHandler(Exception exception, bool send);
        public delegate void DataHandler(UdpPortHandler handler, UdpReceiveResult result);

        readonly int _port;
        public UdpPortHandler(int port, DataHandler handlerDelegate, ExceptionHandler errorHandler)
        {
            _port = port;
            _exceptionHandlerDelegate = errorHandler;
            _handlerDelegate = handlerDelegate;
        }

        public void Start()
        {
            _client = new UdpClient(_port);
            _client.ReceiveAsync().ContinueWith(OnReceive);
        }

        public void Stop()
        {
            _client?.Dispose();
            _client = null;
        }

        public void Send(byte[] bytes, IPEndPoint point)
        {
            try
            {
                var sended = _client?.Send(bytes, bytes.Length, point);

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
                _exceptionHandlerDelegate(ex, true);
            }
        }

        void OnReceive(Task<UdpReceiveResult> task)
        {
            try
            {
                if (task.IsFaulted)
                    throw task.Exception.GetInnerException();

                _handlerDelegate(this, task.Result);
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
                _exceptionHandlerDelegate(ex, false);
            }
            finally
            {
                _client?.ReceiveAsync().ContinueWith(OnReceive);
            }
        }
    }
}
