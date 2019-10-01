using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class UdpPortHandler
    {
        UdpClient _client;

        ExceptionHandler _exceptionHandlerDelegate;
        DataHandler _handlerDelegate;

        public delegate void ExceptionHandler(Exception exception, bool send);
        public delegate void DataHandler(UdpPortHandler handler, UdpReceiveResult result);
        public UdpPortHandler(int port, DataHandler handlerDelegate, ExceptionHandler errorHandler)
        {
            _exceptionHandlerDelegate = errorHandler;
            _handlerDelegate = handlerDelegate;
            _client = new UdpClient(port);
            _client.ExclusiveAddressUse = true;

            _client.ReceiveAsync().ContinueWith(OnReceive);
        }

        public void Send(byte[] bytes, IPEndPoint point)
        {
            try
            {
                var sended = _client.Send(bytes, bytes.Length, point);

                if (sended != bytes.Length)
                    throw new Exception("Sended inconsystency data");
            }
            catch(Exception ex)
            {
                _exceptionHandlerDelegate(ex, true);
            }
        }

        void OnReceive(Task<UdpReceiveResult> task)
        {
            try
            {
                _handlerDelegate(this, task.Result);
            }
            catch(Exception ex)
            {
                _exceptionHandlerDelegate(ex, false);
            }
            finally
            {
                _client.ReceiveAsync().ContinueWith(OnReceive);
            }
        }
    }
}
