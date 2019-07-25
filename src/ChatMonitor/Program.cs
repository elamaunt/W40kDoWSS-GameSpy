using System;
using System.Linq;
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
            var str = "??   &   {f29e528f-2461-4387-9443-84db458a8592}    B a m b o c h u k                   K04W        B a m b o c h u k     ?????      ?     ??BK04W   g i g a m o k       ???      ?       dxp2   1.0    :~?";

            var bytesString = @"254 254 0 0 2 0 2 4 1 38 0 0 0 123 102 50 57 101 53 50 56 102 45 50 52 54 49 45 52 51 56 55 45 57 52 52 51 45 56 52 100 98 52 53 56 97 56 53 57 50 125 9 0 0 0 66 0 97 0 109 0 98 0 111 0 99 0 104 0 117 0 107 0 0 0 0 0 2 0 0 0 4 0 0 0 0 0 0 0 0 2 0 0 0 2 75 48 52 87 9 0 0 0 66 0 97 0 109 0 98 0 111 0 99 0 104 0 117 0 107 0 1 1 0 0 0 0 192 168 159 128 224 23 0 0 0 0 127 0 0 1 224 23 0 0 0 0 0 216 1 233 66 75 48 52 87 7 0 0 0 103 0 105 0 103 0 97 0 109 0 111 0 107 0 0 0 0 0 0 0 192 168 1 31 224 23 0 0 0 0 127 0 0 1 224 23 20 0 0 0 20 0 4 0 0 0 100 120 112 50 3 0 0 0 49 46 48 0 0 0 0 19 58 126 222";
            var bytes = bytesString.Split(" ").Select(x => byte.Parse(x)).ToArray();

            var c = (byte)'}';
            var b = (byte)'B';

            var index = Array.IndexOf(bytes, c);

            index++;

            for (int i = index; i < bytes.Length-4; i++)
            {
                if (bytes[i] == 'K' &&
                    bytes[i+1] == '0' &&
                    bytes[i+2] == '4' &&
                    bytes[i+3] == 'W')
                {
                    var nickLength = bytes[i+4];

                    var nickStart = i + 4 + 3;
                    var nickEnd = nickStart + (nickLength << 1);
                    
                    var nick = BetweenAsChars(bytes, nickStart, nickEnd);

                    var pointStart = nickEnd + 7;
                    var pointEnd = pointStart + 6;

                    var ipEndPoint = Between(bytes, pointStart, pointEnd);

                    Console.WriteLine(nick);
                    Console.WriteLine(ipEndPoint);
                    // Console.WriteLine(Between(bytes, i + 4, i + 4 + 57));
                }
            }

            //K04W

            /*var length = bytes[index++];
            var nickEnd = index + 3 + length * 2;
            
            var nick = BetweenAsChars(bytes, index + 3, nickEnd);

            var index2 = Array.IndexOf(bytes, (byte)'K', nickEnd);

            Console.WriteLine(nick);

            Console.WriteLine(Between(bytes, nickEnd +1, index2));*/


            //var index2 = Array.IndexOf(bytes, b, index);



            //Console.WriteLine(Between(bytes, index, index2));
            int t = 0;
            /* _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
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
             }*/
        }

        private static string BetweenAsChars(byte[] bytes, int index, int index2)
        {
            var bytesClone = new byte[index2 - index];

            for (int i = 0; i < index2 - index; i+=2)
            {
                bytesClone[i] = bytes[index + i + 1];
                bytesClone[i + 1] = bytes[index + i];
            }

            return Encoding.Unicode.GetString(bytesClone);

           // return string.Join("", bytes.Skip(index).Take(index2 - index).Select(x => ((char)x).ToString())) + "END";
        }

        private static string Between(byte[] bytes, int index, int index2)
        {
            return string.Join(" ", bytes.Skip(index).Take(index2 - index).Select(x => x.ToString()));
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
