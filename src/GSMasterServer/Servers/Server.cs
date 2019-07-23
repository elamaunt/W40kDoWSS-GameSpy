using System;
using System.Globalization;

namespace GSMasterServer.Servers
{
    internal class Server
    {
        static object LOCK = new object();

        public static void Log(string tag, string message)
        {
          //  if (tag != Servers.ServerListReport.Category)
          //      return;

            Log(tag +":"+ message);
        }
        public static void Log(string message)
        {
            lock (LOCK)
            {
                var mainRooms = ChatServer.IrcDaemon.GetMainRooms();

                for (int i = 0; i < mainRooms.Length; i++)
                {
                    var room = mainRooms[i];

                    foreach (var item in room.Users)
                        item.WriteServerPrivateMessage(message);
                }
            }

            Console.WriteLine(String.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), message));
        }

        public static void LogError(string tag, string message)
        {
            LogError(tag + ":" + message);
        }

        public static void LogError(string message)
        {
            Log(message);
           /* ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(String.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), message));
            Console.ForegroundColor = c;*/
        }
    }
}