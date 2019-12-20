using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using SoulstormRandomTools;

namespace GSMasterServer.DiscordBot.Commands
{
    internal class RandomCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.User;

        private readonly bool _isMapRandomizer;
        private readonly Randomizer _randomizer = new Randomizer();
        public RandomCommand(bool isMapRandomizer)
        {
            _isMapRandomizer = isMapRandomizer;
        }

        public async Task Execute(SocketMessage socketMessage)
        {
            var commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            byte count = 1;
            if (paramCount >= 1)
            {
                byte.TryParse(commandParams[0], out count);
                if (count >= 100)
                    count = 100;
            }

            string[] data = null;
            if (paramCount >= 2)
                data = commandParams.Skip(1).ToArray();

            var generatedItems = _isMapRandomizer ? _randomizer.GenerateRandomMaps(count, data) : _randomizer.GenerateRandomRaces(count, data);
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
