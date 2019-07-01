using System.Text;

namespace ChatMonitor
{
    public static class ByteHelpers
    {
        public static byte[] ToAssciiBytes(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            byte[] bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);

            return bytes;
        }
    }
}
