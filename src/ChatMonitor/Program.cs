using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static ChatMonitor.Constants;

namespace ChatMonitor
{
    class Program
    {
        static Socket _socket;
        static byte[] _readbuffer;

        static void Main(string[] args)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 5000,
                ReceiveTimeout = 5000,
                SendBufferSize = 65535,
                ReceiveBufferSize = 65535
            };

            _socket.Connect(new IPEndPoint(Dns.GetHostEntry(NETWORK).AddressList[0], PORT));
            
            _socket.Send($"NICK {NICK}\r\n".ToAssciiBytes());
            _socket.Send($"USER {IDENTD} {NETWORK} bla :{REALNAME}\r\n".ToAssciiBytes());
            
            _readbuffer = new byte[8096];

            Console.WriteLine("START MAIN LOOP");

            BeginReceive();

            while (true)
            {
                Console.WriteLine("Enter STOP to break the loop");
                if ("STOP" == Console.ReadLine())
                    break;
            }
        }

        private static void BeginReceive()
        {
            try
            {
                _socket.BeginReceive(_readbuffer, 0, _readbuffer.Length, SocketFlags.None, OnDataReceived, null);
            }
            catch (SocketException ex)
            {
                CONNECTED = false;
                Console.WriteLine(ex);
                Thread.Sleep(10000);
                Main(null);
            }
        }

        private static void OnDataReceived(IAsyncResult ar)
        {
            var received = _socket.EndReceive(ar);

            var str = Encoding.ASCII.GetString(_readbuffer, 0, received);
            var split = str.Split('\n');

            for (int i = 0; i < split.Length; i++)
            {
                var line = split[i];

                Console.WriteLine("[IN]" + line);

                if (!line.Contains("PING") && CONNECTED && line.Contains("PRIVMSG"))
                {
                    // user = line[0].split("!", 1)
                    // user = user[0]
                    // channel = line[2]
                    // msg = line[3:]
                    // print "[IN][%s][%s]%s" % (user, channel, ' '.join(msg));
                }

                if (line.Contains("PING"))
                {
                    _socket.Send($"PONG {line.Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]}\r\n".ToAssciiBytes());
                }

                if (line.Contains("372"))
                {
                    CONNECTED = true;
                   // OperArcade(OPER_NAME, OPER_EMAIL, OPER_PASSWORD);
                    Thread.Sleep(5);
                    LoadStartChannels(CHAN);
                }

                BeginReceive();
            }
        }

        private static void LoadStartChannels(string[] channels)
        {
            for (int i = 0; i < channels.Length; i++)
            {
                try
                {
                    _socket.Send($"JOIN {channels[i]}\r\n".ToAssciiBytes());
                }
                catch (Exception)
                {
                }
            }
        }

        private static void OperArcade(string name, string email, string password)
        {
            try
            {
                _socket.Send($"OPER {name} {email} {password}\r\n".ToAssciiBytes());
            }
            catch (Exception)
            {
            }
        }
    }
}
