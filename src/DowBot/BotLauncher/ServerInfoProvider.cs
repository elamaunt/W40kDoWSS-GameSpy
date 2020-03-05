using System;
using DiscordBot.Commands.DynamicModule;

namespace BotLauncher
{
    public class ServerInfoProvider: IDynamicDataProvider
    {
        public string Text => DateTime.UtcNow.ToString();
        public ulong ChannelId { get; } = 682269859895443456;
    }
}