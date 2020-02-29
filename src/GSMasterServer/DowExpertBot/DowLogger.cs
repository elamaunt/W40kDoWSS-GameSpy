using System;
using DiscordBot;
using DiscordBot.Communication;
using IrcNet.Tools;

namespace GSMasterServer.DowExpertBot
{
    public class DowLogger: IDowLogger
    {
        public void Write(object obj, DowBotLogLevel logLevel)
        {
            switch (logLevel)
            {
                case DowBotLogLevel.Trace:
                    Logger.Trace(obj);
                    break;
                case DowBotLogLevel.Debug:
                    Logger.Debug(obj);
                    break;
                case DowBotLogLevel.Info:
                    Logger.Info(obj);
                    break;
                case DowBotLogLevel.Warn:
                    Logger.Warn(obj);
                    break;
                case DowBotLogLevel.Error:
                    Logger.Error(obj);
                    break;
                case DowBotLogLevel.Fatal:
                    Logger.Fatal(obj);
                    break;
            }
        }
    }
}