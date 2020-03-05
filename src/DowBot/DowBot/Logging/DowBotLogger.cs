using DiscordBot.Communication;

namespace DiscordBot
{
    public static class DowBotLogger
    {
        public static void Fatal(object obj)
        {
            Write(obj, DowBotLogLevel.Fatal);
        }

        //LogLevel: 5
        public static void Error(object obj)
        {
            Write(obj, DowBotLogLevel.Error);
        }

        //LogLevel: 4
        public static void Warn(object obj)
        {
            Write(obj, DowBotLogLevel.Warn);
        }

        //LogLevel: 3
        public static void Info(object obj)
        {
            Write(obj, DowBotLogLevel.Info);
        }

        //LogLevel: 2
        public static void Debug(object obj)
        {
            Write(obj, DowBotLogLevel.Debug);
        }

        //LogLevel: 1
        public static void Trace(object obj)
        {
            Write(obj, DowBotLogLevel.Trace);
        }


        internal static IDowLogger Logger;
        private static void Write(object obj, DowBotLogLevel logLevel)
        {
            Logger?.Write(obj, logLevel);
        }
    }
}