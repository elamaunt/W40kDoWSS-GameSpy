namespace DiscordBot.Communication
{
    public interface IDowLogger
    {
        void Write(object obj, DowBotLogLevel logLevel);
    }
}