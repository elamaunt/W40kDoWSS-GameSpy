using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ChatMonitor.Constants;

namespace ChatMonitor
{
    class Program
    {
        static Socket _socket;
        static byte[] _readbuffer;



        static unsafe void Main(string[] args)
        {
            var input = "-623636148<100000005><100000021>-623636148<100000005><100000021> START PROCESSING-623636148<100000005><100000021> PROCESSED COUNT-623636148<100000005><100000021> PROCESSED FULL-623636148<100000005><100000021>-623636148<100000005><100000021> ALREADY PROCESSED-402364039<100000005><100000010><100000021><100000032>-402364039<100000005><100000010><100000021><100000032> START PROCESSING-402364039<100000005><100000010><100000021><100000032> PROCESSED COUNT-402364039<100000005><100000010><100000021><100000032> PROCESSED FULL-402364039<100000005><100000010><100000021><100000032>-402364039<100000005><100000010><100000021><100000032> ALREADY PROCESSED1536383770<100000032><100000005>1536383770<100000032><100000005> START PROCESSING1536383770<100000032><100000005> PROCESSED COUNT1536383770<100000032><100000005> PROCESSED FULL573098932<100000015><100000036>573098932<100000015><100000036> START PROCESSING573098932<100000015><100000036> PROCESSED COUNT573098932<100000015><100000036> INFO NULL573098932<100000015><100000036> PROCESSED FULL-918241492<100000002><100000001>-918241492<100000002><100000001> START PROCESSING-918241492<100000002><100000001> PROCESSED COUNT-918241492<100000002><100000001> PROCESSED FULL-918241492<100000002><100000001>-918241492<100000002><100000001> ALREADY PROCESSED-1379414417<100000001><100000002>-1379414417<100000001><100000002> START PROCESSING-1379414417<100000001><100000002> PROCESSED COUNT-1379414417<100000001><100000002> PROCESSED FULL-1379414417<100000001><100000002>-1379414417<100000001><100000002> ALREADY PROCESSED1693668395<100000015><100000031>1693668395<100000015><100000031> START PROCESSING1693668395<100000015><100000031> PROCESSED COUNT1693668395<100000015><100000031> PROCESSED FULL2039550101<100000037><100000010>2039550101<100000037><100000010> START PROCESSING2039550101<100000037><100000010> PROCESSED COUNT2039550101<100000037><100000010> PROCESSED FULL2039550101<100000037><100000010>2039550101<100000037><100000010> ALREADY PROCESSED-113069280<100000031><100000015>-113069280<100000031><100000015> START PROCESSING-113069280<100000031><100000015> PROCESSED COUNT-113069280<100000031><100000015> PROCESSED FULL880375604<100000001><100000002>880375604<100000001><100000002> START PROCESSING880375604<100000001><100000002> PROCESSED COUNT880375604<100000001><100000002> PROCESSED FULL880375604<100000001><100000002>880375604<100000001><100000002> ALREADY PROCESSED785540362<100000036><100000015>785540362<100000036><100000015> START PROCESSING785540362<100000036><100000015> PROCESSED COUNT785540362<100000036><100000015> PROCESSED FULL170705963<100000010><100000012>170705963<100000010><100000012> START PROCESSING170705963<100000010><100000012> PROCESSED COUNT170705963<100000010><100000012> INFO NULL170705963<100000010><100000012> PROCESSED FULL170705963<100000010><100000012>170705963<100000010><100000012> ALREADY PROCESSED1070512943<100000031><100000002>1070512943<100000031><100000002> START PROCESSING1070512943<100000031><100000002> PROCESSED COUNT1070512943<100000031><100000002> PROCESSED FULL1070512943<100000031><100000002>1070512943<100000031><100000002> ALREADY PROCESSED-563177090<100000031><100000010>-563177090<100000031><100000010> START PROCESSING-563177090<100000031><100000010> PROCESSED COUNT-563177090<100000031><100000010> PROCESSED FULL-563177090<100000031><100000010>-563177090<100000031><100000010> ALREADY PROCESSED-1314532130<100000038><100000015>-1314532130<100000038><100000015> START PROCESSING-1314532130<100000038><100000015> PROCESSED COUNT-1314532130<100000038><100000015> PROCESSED FULL";



            input = input.Replace("START PROCESSING", "START PROCESSING\n\r");
            input = input.Replace("PROCESSED COUNT", "PROCESSED COUNT\n\r");
            input = input.Replace("PROCESSED FULL", "PROCESSED FULL\n\r");
            input = input.Replace("ALREADY PROCESSED", "ALREADY PROCESSED\n\r");
            input = input.Replace("INFO NULL", "INFO NULL\n\r");

            var values = input.Split("\n\r");

            File.AppendAllLines("Parsed.txt", values);


            /* var Gamekey = "pXL838".ToAssciiBytes();

             var chall = "0000000000000000".ToAssciiBytes();

             var serverKey = new ChatCrypt.GDCryptKey();
             var serverKeyClone = new ChatCrypt.GDCryptKey();

             fixed (byte* challPtr = chall)
             {
                 fixed (byte* gamekeyPtr = Gamekey)
                 {
                     ChatCrypt.GSCryptKeyInit(serverKey, challPtr, gamekeyPtr, Gamekey.Length);
                     ChatCrypt.GSCryptKeyInit(serverKeyClone, challPtr, gamekeyPtr, Gamekey.Length);
                 }
             }

             var bytes = new byte[] { 1, 2, 3 };

             fixed (byte* bytesToSendPtr = bytes)
             {
                 ChatCrypt.GSEncodeDecode(serverKey, bytesToSendPtr, bytes.Length);

                 ChatCrypt.GSEncodeDecode(serverKeyClone, bytesToSendPtr, bytes.Length);
             }

             int i = 0;*/
            // int a = 1234;
            // int b = 8768;

            // 127.0.0.1:6112
            //  Mllaal1K9M

            /*var loopbackBytes = IPAddress.Loopback.GetAddressBytes();
            var ip1 = BitConverter.ToInt32(loopbackBytes, 0);
            var ip2 = BitConverter.ToInt32(loopbackBytes.Reverse().ToArray(), 0);

            var buffer = new byte[32];

            buffer = ChatCrypt.PiStagingRoomHash(ip1, ip1, 6112, buffer);*/


            //var ip = ChatCrypt.ConvertFromIpAddressToInteger("127.0.0.1");
            //var ip2 = ChatCrypt.ConvertFromIpAddressToInteger("192.168.159.1");

            //ip = BitConverter.ToUInt32(BitConverter.GetBytes(ip).Reverse().ToArray(), 0);
            //ip2 = BitConverter.ToUInt32(BitConverter.GetBytes(ip2).Reverse().ToArray(), 0);
            //var val = Encoding.ASCII.GetString(ChatCrypt.PiStagingRoomHash(ip, ip2, 6112));


            //ChatCrypt.DecodeStagingRoomHash("llaal1K9", ip, 6112);

            /* var pb = BitConverter.GetBytes((ushort)6112);

             pb = pb.Reverse().ToArray();

             var port = BitConverter.ToUInt16(pb, 0);

             var buffer = new byte[32];*/



            // var bytes1 = "Mllaal1K9M".ToAssciiBytes();
            // var bytes2 = Encoding.UTF8.GetBytes("Mllaal1K9M");

            /*  var ip1 = ConvertFromIntegerToIpAddress(ChatCrypt.DecodeIP(bytes1, true));
              var ip2 = ChatCrypt.DecodeIP(bytes1, false);
              var ip3 = ChatCrypt.DecodeIP(bytes2, true);
              var ip4 = ChatCrypt.DecodeIP(bytes2, false);*/


            //var ids = LoadSteamIds(new List<string>() { "elamaunt" }).Result;

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
                      bytes[i+2] == '4')
                  {
                      if (bytes[i + 3] != 'W')
                         i--;

                      var nickLength = bytes[i+4];

                      var nickStart = i + 4 + 3;
                      var nickEnd = nickStart + (nickLength << 1);

                      var nick = BetweenAsChars(bytes, nickStart, nickEnd);

                      var pointStart = nickEnd + 7;
                      var pointEnd = pointStart + 6;

                      var ipEndPoint = Between(bytes, pointStart, pointEnd);

                      //Console.WriteLine(nick);
                      //Console.WriteLine(ipEndPoint);
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
                //Console.WriteLine(ex);
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

                //Console.WriteLine("[IN]" + line);

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

        private static async Task<Dictionary<string, ulong>> LoadSteamIds(List<string> nicks)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            for (int i = 0; i < nicks.Count; i++)
                writer.Write(nicks[i]);

            var buffer = ms.GetBuffer();

            var client = new UdpClient();

           // var endPoint = new IPEndPoint(IPAddress.Parse("134.209.198.2"), 27902);
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27902);

            await client.SendAsync(buffer, buffer.Length, endPoint);
            var result = await client.ReceiveAsync();

            ms = new MemoryStream(result.Buffer);
            var reader = new BinaryReader(ms);

            var ids = new Dictionary<string, ulong>();

            while (ms.Position + 1 < ms.Length)
                ids[reader.ReadString()] = reader.ReadUInt64();

            return ids;
        }
    }
}
