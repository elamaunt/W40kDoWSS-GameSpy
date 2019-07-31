using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace GSMasterServer.Utils
{
    public unsafe static class ChatCrypt
    {
        /// <summary>
        /// gs_peerchat_ctx same
        /// </summary>
        public class GDCryptKey
        {
            /// <summary>
            /// gs_peerchat_1 same
            /// </summary>
            public byte X;

            /// <summary>
            ///  gs_peerchat_2 same
            /// </summary>
            public byte Y;

            /// <summary>
            /// gs_peerchat_crypt same
            /// </summary>
            public byte[] State = new byte[256];
        }

        static void SwapByte(byte* array, int index1, int index2)
        {
            var t = array[index1];
            array[index1] = array[index2];
            array[index2] = t;
        }

        // gs_peerchat same
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void GSEncodeDecode(GDCryptKey ctx, byte* data, int size)
        {
            byte num1, num2, t;

            fixed (byte* crypt = ctx.State)
            {
                num1 = ctx.X;
                num2 = ctx.Y;

                while (size-- > 0)
                {
                    t = crypt[++num1];
                    num2 += t;
                    crypt[num1] = crypt[num2];
                    crypt[num2] = t;
                    t += crypt[num1];
                    *data++ ^= crypt[t];
                }

                ctx.X = num1;
                ctx.Y = num2;
            }
        }


        // gs_peerchat init same
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void GSCryptKeyInit(GDCryptKey ctx, byte* chall, byte* gamekey, int gameKeySize)
        {
            byte* challenge = stackalloc byte[16];
            byte* l;
            byte* l1;
            byte* p;
            byte* p1;
            byte t;
            byte t1;

            ctx.X = 0;
            ctx.Y = 0;

            // Setup start 'M' chars to make it same with C code
            for (int i = 0; i < 16; i++)
                challenge[i] = 204;

            fixed (byte* crypt = ctx.State)
            {

                p = challenge;
                l = challenge + 16;
                p1 = gamekey;
                int counter = 0;

                do
                {
                    if (counter == gameKeySize)
                    {
                        p1 = gamekey;
                        counter = 0;
                    }

                    counter++;

                    *(p++) = (byte)(*(chall++) ^ *(p1++));
                }
                while (p != l);


                t1 = 255;
                p1 = crypt;
                l1 = crypt + 256;
                do
                {
                    *p1++ = t1--;
                }
                while (p1 != l1);

                t1++;       // means t1 = 0;
                p = crypt;
                p1 = challenge;

                do
                {
                    t1 = (byte)(t1 + *p1 + *p);
                    t = crypt[t1];
                    crypt[t1] = *p;
                    *p = t;
                    p++;
                    p1++;

                    if (p1 == l)
                        p1 = challenge;
                }
                while (p != l1);
            }
        }

        const bool piOldMangleStagingRooms = true;
        static readonly byte[] digits_hex = "0123456789abcdef".ToAssciiBytes();
        static readonly byte[] digits_crypt = "aFl4uOD9sfWq1vGp".ToAssciiBytes();
        static readonly byte[] new_digits_crypt = "qJ1h4N9cP3lzD0Ka".ToAssciiBytes();
        const uint ip_xormask = 0xc3801dc7;
        static byte[] cryptbuffer = new byte[8];

        public static string PiStagingRoomHash(string publicIP, string privateIP, ushort port)
        {
            var ip = ConvertFromIpAddressToInteger(publicIP);
            var ip2 = ConvertFromIpAddressToInteger(privateIP);

            return $"M{Encoding.ASCII.GetString(PiStagingRoomHash(ip, ip2, port))}M";
        }

        public static byte[] PiStagingRoomHash(uint publicIP, uint privateIP, ushort port)
        {
            uint result;

            publicIP = (uint)IPAddress.NetworkToHostOrder((int)publicIP);
            privateIP = (uint)IPAddress.NetworkToHostOrder((int)privateIP);

            result = (((privateIP >> 24) & 0xFF) | ((privateIP >> 8) & 0xFF00) | ((privateIP << 8) & 0xFF0000) | ((privateIP << 24) & 0xFF000000));
            result ^= publicIP;
            result ^= (port | ((uint)port << 16));

            return EncodeIP(result, true);
        }

        public static uint DecodeIP(byte[] buffer, bool newCrypt)
        {
            byte[] crypt = newCrypt ? new_digits_crypt : digits_crypt;
            uint ip = 0;
            int digit_idx;
            int i;

            for (i = 0; i < 8; i++)
            {
                digit_idx = Array.IndexOf(crypt, buffer[i]);

                if ((digit_idx < 0) || (digit_idx > 15))
                    return 0;

                cryptbuffer[i] = digits_hex[digit_idx];
            }

            // Cap the buffer.
            cryptbuffer[i] = (byte)'\0';

            var array = cryptbuffer.Where(x => x != 0).Select(x => Convert.ToByte(Encoding.ASCII.GetString(new byte[] { x }), 16)).ToArray();

            ip = BitConverter.ToUInt32(array, 0);

            // Convert the string to an unsigned long (the XORd ip addr).
            // sscanf(cryptbuffer, "%x", &ip);

            //ip = 

            // re-XOR the IP address.
            ip ^= ip_xormask;

            return ip;
        }

        public static byte[] EncodeIP(long ip, bool newCrypt)
        {
            byte[] crypt = newCrypt ? new_digits_crypt : digits_crypt;
            int i;
            int digit_idx;

            // XOR the IP address.
            ip ^= ip_xormask;

            // Print out the ip addr in hex form.

            var hex = ip.ToString("X").ToLowerInvariant();
            var hexBytes = hex.ToAssciiBytes();

            Array.Copy(hexBytes, cryptbuffer, hexBytes.Length);
            //sprintf(cryptbuffer, "%08x", ip);

            // Translate chars in positions 0 through 7 from hex digits to "crypt" digits.
            for (i = 0; i < 8; i++)
            {
                //str = Array.IndexOf(digits_hex, cryptbuffer[i]);
                //digit_idx = (str - digits_hex);
                digit_idx = Array.IndexOf(digits_hex, cryptbuffer[i]);

                if ((digit_idx < 0) || (digit_idx > 15)) // sanity check
                {
                    var b = "14saFv19".ToAssciiBytes();
                    Array.Copy(b, cryptbuffer, b.Length);
                    break;
                }

                cryptbuffer[i] = crypt[digit_idx];
            }

            return (byte[])cryptbuffer.Clone();
        }

        public static uint ConvertFromIpAddressToInteger(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            byte[] bytes = address.GetAddressBytes().ToArray();

            // flip big-endian(network order) to little-endian
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string ConvertFromIntegerToIpAddress(uint ipAddress)
        {
            byte[] bytes = BitConverter.GetBytes(ipAddress);

            // flip little-endian to big-endian(network order)
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return new IPAddress(bytes).ToString();
        }
    }
}
