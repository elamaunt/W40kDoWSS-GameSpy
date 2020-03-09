using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;
using RandomTools;
using RandomTools.Types;

namespace DiscordBot.Commands.RandomModule
{
    internal class RandomCommand: GuildCommand, ICommandDescription
    {
        private readonly DowItemType _dowItemType;
        private readonly Randomizer _randomizer;

        private void FillTags(ref StringBuilder sb)
        {
            var items = _dowItemType == DowItemType.Race ? _randomizer.ItemsProvider.Races : _randomizer.ItemsProvider.Maps;
            foreach (var item in items.Where(x => x.ItemType == _dowItemType))
            {
                sb.AppendLine(item.Key + ": " + item.EnglishName);
            }
        }

        public string RuDescription
        {
            get
            {
                var sb = new StringBuilder();
                switch (_dowItemType)
                {
                    case DowItemType.Race:
                        sb.AppendLine("Эта команда генерирует случайные расы.\nИспользование: !rr + [кол-во рас] + [список тегов рас для включения]");
                        break;
                    case DowItemType.Map2p:
                        sb.AppendLine("Эта команда генерирует карты для 2 игроков.\nИспользование: !rm2 + [кол-во карт] + [список тегов карт для включения]");
                        break;
                    case DowItemType.Map4p:
                        sb.AppendLine("Эта команда генерирует карты для 4 игроков.\nИспользование: !rm4 + [кол-во карт] + [список тегов карт для включения]");
                        break;
                    case DowItemType.Map6p:
                        sb.AppendLine("Эта команда генерирует карты для 6 игроков.\nИспользование: !rm6 + [кол-во карт] + [список тегов карт для включения]");
                        break;
                    case DowItemType.Map8p:
                        sb.AppendLine("Эта команда генерирует карты для 8 игроков.\nИспользование: !rm8 + [кол-во карт] + [список тегов карт для включения]");
                        break;
                }

                sb.AppendLine("Область действия: Discord сервер");
                sb.AppendLine("Список допустимых тегов: ");
                FillTags(ref sb);

                return sb.ToString();
            }
        }
        
        public string EnDescription
        {
            get
            {
                var sb = new StringBuilder();
                switch (_dowItemType)
                {
                    case DowItemType.Race:
                        sb.AppendLine("This command generates random races.\nUsage: !rr + [races count] + [list of races tags for including in random]");
                        break;
                    case DowItemType.Map2p:
                        sb.AppendLine("This command generates random maps for 2 players.\nUsage: !rm2 + [maps count] + [list of maps tags for including in random]");
                        break;
                    case DowItemType.Map4p:
                        sb.AppendLine("This command generates random maps for 4 players.\nUsage: !rm4 + [maps count] + [list of maps tags for including in random]");
                        break;
                    case DowItemType.Map6p:
                        sb.AppendLine("This command generates random maps for 6 players.\nUsage: !rm6 + [maps count] + [list of maps tags for including in random]");
                        break;
                    case DowItemType.Map8p:
                        sb.AppendLine("This command generates random maps for 8 players.\nUsage: !rm8 + [maps count] + [list of maps tags for including in random]");
                        break;
                }
                
                sb.AppendLine("Usage area: Discord server");
                sb.AppendLine("Tags list: ");
                FillTags(ref sb);

                return sb.ToString();
            }
        }

        public RandomCommand(DowItemType dowItemType, IDowItemsProvider dowItemsProvider, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _dowItemType = dowItemType;
            _randomizer = new Randomizer(dowItemsProvider);
        }
        
        public override async Task Execute(SocketMessage socketMessage, bool isRus)
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
            var items = isRus ? generatedItems.Select(x => x.RussianName) : generatedItems.Select(x => x.EnglishName);
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