﻿using GSMasterServer.Servers;
using System;
using System.Net;
using System.Threading;

namespace GSMasterServer
{
    class Program
	{

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        static void Main(string[] args)
        {
            args = new string[] { "+db", @"test.db" };
            
            IPAddress bind = IPAddress.Any;

            if (args.Length >= 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("+bind"))
                    {
                        if ((i >= args.Length - 1) || !IPAddress.TryParse(args[i + 1], out bind))
                        {
                            logger.Error("+bind value must be a valid IP Address to bind to!");
                        }
                    }
                    else if (args[i].Equals("+db"))
                    {
                        if ((i >= args.Length - 1))
                        {
                            logger.Error("+db value must be a path to the database");
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
                logger.Error("Error initializing database, please confirm parameter +db is valid");
                logger.Error("Press any key to continue");
                Console.ReadKey();
                return;
            }
            logger.Info("Database successful initialized");

            CDKeyServer cdKeyServer = new CDKeyServer(bind, 29910);
            //ServerListReport serverListReport = new ServerListReport(bind, 27900);
            //ServerNatNeg serverNatNeg = new ServerNatNeg(bind, 27901);
            ServerSteamIdsRetrieve serverSteamIdsRetrieve = new ServerSteamIdsRetrieve(bind, 27902);
            //ServerListRetrieve serverListRetrieve = new ServerListRetrieve(bind, 28910, serverListReport);
            //LoginServer loginServer = new LoginServer(bind, 29900, 29901);
            LoginServer loginServer = new LoginServer(bind, 29902, 29903);
            ChatServer chatServer = new ChatServer(bind, 6668);
            HttpServer httpServer = new HttpServer(bind, 80);
            StatsServer statsServer = new StatsServer(bind, 29920);

            while (true)
                Thread.Sleep(1000);
        }
	}
}
