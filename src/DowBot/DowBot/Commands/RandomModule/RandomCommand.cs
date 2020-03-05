using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;
using RandomTools;
using RandomTools.Types;

namespace DiscordBot.Commands.RandomModule
{
    internal class RandomCommand: GuildCommand
    {
        private readonly DowItemType _dowItemType;
        private readonly Randomizer _randomizer;

        public RandomCommand(DowItemType dowItemType, IDowItemsProvider dowItemsProvider, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _dowItemType = dowItemType;
            _randomizer = new Randomizer(dowItemsProvider);
        }
        
        public override async Task Execute(SocketMessage socketMessage)
        {
            var commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            byte count = 1;
            if (paramCount >= 1)
            {
                byte.TryParse(commandParams[0], out count);
                if (count >= 30)
                    count = 30;
            }

            string[] data = null;
            if (paramCount >= 2)
                data = commandParams.Skip(1).ToArray();

            var generatedItems = _randomizer.GenerateRandomItems(_dowItemType, count, data);
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