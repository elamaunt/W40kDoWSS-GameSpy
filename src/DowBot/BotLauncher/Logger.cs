using System;
using DiscordBot;
using DiscordBot.Communication;

namespace BotLauncher
{
    public class Logger: IDowLogger
    {
        public void Write(object obj, DowBotLogLevel logLevel)
        {
            Console.WriteLine($"[{logLevel}] {obj}");
        }
    }
}