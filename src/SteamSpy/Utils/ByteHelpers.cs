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
    }
}
