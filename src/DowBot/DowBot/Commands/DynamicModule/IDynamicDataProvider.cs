namespace DiscordBot.Commands.DynamicModule
{
    public interface IDynamicDataProvider
    {
        string Text { get; }
        
        ulong ChannelId { get; }
    }
}