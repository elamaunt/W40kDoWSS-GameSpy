using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;
using RandomTools;
using RandomTools.Types;

namespace DiscordBot.Commands.RandomModule
{
    internal class ShuffleCommand: GuildCommand
    {
        private readonly DowItemType _dowItemType;
        private readonly Randomizer _randomizer;

        public ShuffleCommand(DowItemType dowItemType, IDowItemsProvider dowItemsProvider, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _dowItemType = dowItemType;
            _randomizer = new Randomizer(dowItemsProvider);
        }
        
        public override async Task Execute(SocketMessage socketMessage)
        {
            var commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            if (paramCount < 1)
            {
                DowBotLogger.Trace("ShuffleCommand executed with wrong params! Return.");
                return;
            }

            var generatedItems = _randomizer.ShuffleItems(_dowItemType, commandParams);
            if (generatedItems.Length == 0)
                return;
            var items = generatedItems.Select(x => x.EnglishName);
            var sb = new StringBuilder();

            var i = 0;
            foreach (var item in items)
            {
                sb.AppendLine($"{++i}. {item}");
            }

            await socketMessage.Channel.SendMessageAsync(sb.ToString());
        }
        
    }
}