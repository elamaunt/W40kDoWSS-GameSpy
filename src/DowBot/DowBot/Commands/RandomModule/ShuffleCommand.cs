using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;
using RandomTools;
using RandomTools.Types;

namespace DiscordBot.Commands.RandomModule
{
    internal class ShuffleCommand: GuildCommand, ICommandDescription
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
                        sb.AppendLine("Эта команда перемешивает расы.\nИспользование: !sr + [список тегов рас для перемешивания]");
                        break;
                    case DowItemType.Map2p:
                        sb.AppendLine("Эта команда перемешивает карты для 2х игроков.\nИспользование: !sm2 + [список тегов карт для перемешивания]");
                        break;
                    case DowItemType.Map4p:
                        sb.AppendLine("Эта команда перемешивает карты для 4х игроков.\nИспользование: !sm4 + [список тегов карт для перемешивания]");
                        break;
                    case DowItemType.Map6p:
                        sb.AppendLine("Эта команда перемешивает карты для 6х игроков.\nИспользование: !sm6 + [список тегов карт для перемешивания]");
                        break;
                    case DowItemType.Map8p:
                        sb.AppendLine("Эта команда перемешивает карты для 8х игроков.\nИспользование: !sm8 + [список тегов карт для перемешивания]");
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
                        sb.AppendLine("This command shuffles races.\nUsage: !sr + [list of races tags to shuffle]");
                        break;
                    case DowItemType.Map2p:
                        sb.AppendLine("This command shuffles maps for 2 players.\nUsage: !sm2 + [list of races tags to shuffle]");
                        break;
                    case DowItemType.Map4p:
                        sb.AppendLine("This command shuffles maps for 4 players.\nUsage: !sm4 + [list of races tags to shuffle]");
                        break;
                    case DowItemType.Map6p:
                        sb.AppendLine("This command shuffles maps for 6 players.\nUsage: !sm6 + [list of races tags to shuffle]");
                        break;
                    case DowItemType.Map8p:
                        sb.AppendLine("This command shuffles maps for 8 players.\nUsage: !sm8 + [list of races tags to shuffle]");
                        break;
                }
                
                sb.AppendLine("Usage area: Discord server");
                sb.AppendLine("Tags list: ");
                FillTags(ref sb);

                return sb.ToString();
            }
        }
        
        public ShuffleCommand(DowItemType dowItemType, IDowItemsProvider dowItemsProvider, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _dowItemType = dowItemType;
            _randomizer = new Randomizer(dowItemsProvider);
        }
        
        public override async Task Execute(SocketMessage socketMessage, bool isRus)
        {
            var commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            if (paramCount <= 0)
                return;

            var generatedItems = _randomizer.ShuffleItems(_dowItemType,  commandParams);
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