namespace DiscordBot.BotParams
{
    public class SyncModuleParams: IModuleParams
    {
        public ulong ChannelId { get; }

        public SyncModuleParams(ulong syncChannelId)
        {
            ChannelId = syncChannelId;
        }
    }
}