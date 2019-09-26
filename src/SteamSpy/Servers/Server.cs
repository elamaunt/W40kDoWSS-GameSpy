using System;
using System.Globalization;

namespace GSMasterServer.Servers
{
    public class Server
    {
        public static void Log(string tag, string message)
        {
          //  if (tag != Servers.ServerListReport.Category)
          //      return;

            Log(tag +":"+ message);
        }
        public static void Log(string message)
        {
            //Console.WriteLine(String.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), message));
        }

        public static void LogError(string tag, string message)
        {
            LogError(tag + ":" + message);
        }

        public static void LogError(string message)
        {
            //ConsoleColor c = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.Error.WriteLine(String.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), message));
            //Console.ForegroundColor = c;
        }
    }
}