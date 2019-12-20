using GSMasterServer.Servers;
using System;
using System.Diagnostics;
using System.IO;

using System.Net;
using System.Threading;
using GSMasterServer.DiscordBot;
using IrcNet.Tools;
using Exception = System.Exception;

namespace GSMasterServer
{
    class Program
	{     
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            args = new[] { "+db", @"ServerData.db" };
            
            IPAddress bind = IPAddress.Any;

            if (args.Length >= 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("+bind"))
                    {
                        if ((i >= args.Length - 1) || !IPAddress.TryParse(args[i + 1], out bind))
                        {
                            Logger.Error("+bind value must be a valid IP Address to bind to!");
                        }
                    }
                    else if (args[i].Equals("+db"))
                    {
                        if ((i >= args.Length - 1))
                        {
                            Logger.Error("+db value must be a path to the database");
                        }
                        else
                        {
                            Database.Initialize(args[i + 1]);
                        }
                    }
                }
            }

            if (!Database.IsInitialized())
            {
                Logger.Fatal("Error initializing database, please confirm parameter +db is valid");
                //Logger.Error("Press any key to continue");
                //Console.ReadKey();
                return;
            }
            Logger.Info("Database successful initialized");
            
            //CDKeyServer cdKeyServer = new CDKeyServer(bind, 29910);
            //ServerListReport serverListReport = new ServerListReport(bind, 27900);
            //ServerNatNeg serverNatNeg = new ServerNatNeg(bind, 27901);
            //ServerSteamIdsRetrieve serverSteamIdsRetrieve = new ServerSteamIdsRetrieve(bind, 27902);
            //ServerListRetrieve serverListRetrieve = new ServerListRetrieve(bind, 28910, serverListReport);
            //LoginServer loginServer = new LoginServer(bind, 29900, 29901);
            //LoginServer loginServer = new LoginServer(bind, 29902, 29903);
            //ChatServer chatServer = new ChatServer(bind, 6668);
            //HttpServer httpServer = new HttpServer(bind, 80);
            //StatsServer statsServer = new StatsServer(bind, 29920);

            var singleServer = new SingleMasterServer();
            try
            {
                var botManager = new BotManager(singleServer);
                botManager.Run().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
            
            while (true)
                Thread.Sleep(1000);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e.ExceptionObject);

            Directory.CreateDirectory("Crashes");
            File.WriteAllText(Path.Combine("Crashes","FatalException-"+DateTime.Now.ToLongTimeString()+".ex"), e.ExceptionObject.ToString());
        }
    }
}
