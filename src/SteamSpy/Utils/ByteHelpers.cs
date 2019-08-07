using System;
using System.Text;

namespace GSMasterServer.Utils
{
    public static class ByteHelpers
    {
        public static byte[] ToUTF8Bytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static byte[] ToAssciiBytes(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            byte[] bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);

            return bytes;
        }

        public static UInt32 ReverseEndian32(UInt32 x)
        {
            //little to big or vice versa
            return (UInt32)(x << 24 | (x << 8 & 0x00ff0000) | x >> 8 & 0x0000ff00 | x >> 24 & 0x000000ff);
        }

        public static UInt16 ReverseEndian16(UInt16 x)
        {
            //little to big or vice versa
            return (UInt16)((x & 0xff00) >> 8 | (x & 0x00ff) << 8);
        }
    }
}
