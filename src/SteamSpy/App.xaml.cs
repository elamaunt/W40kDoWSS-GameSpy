using GSMasterServer.Servers;
using System;
using System.IO;
using System.Net;
using System.Windows;

namespace SteamSpy
{
    public partial class App : Application
    {
        public static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            var bind = IPAddress.Any;

            //CDKeyServer cdKeyServer = new CDKeyServer(bind, 29910);
            ServerListReport serverListReport = new ServerListReport(bind, 27900);
            //ServerRetranslationNatNeg serverNatNeg = new ServerRetranslationNatNeg(bind, 27901);
            ServerListRetrieve serverListRetrieve = new ServerListRetrieve(bind, 28910);

            LoginServerRetranslator loginMasterServer = new LoginServerRetranslator(bind, 29900, 29901);
            /*
             ChatServer chatServer = new ChatServer(bind, 6667);
             HttpServer httpServer = new HttpServer(bind, 80);
             StatsServer statsServer = new StatsServer(bind, 29920);*/

            if (Is64BitProcess)
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api64.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api.dll"), true);
            else
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api86.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api.dll"), true);
            base.OnStartup(e);
        }
    }
}
