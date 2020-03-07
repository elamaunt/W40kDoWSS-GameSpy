using System;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot;
using DiscordBot.Commands;
using DiscordBot.Commands.Primitives;
using GSMasterServer.Servers;

namespace GSMasterServer.DowExpertBot
{
    public class GetPlayerCommand: DmCommand
    {
        private readonly SingleMasterServer _singleMasterServer;
        public GetPlayerCommand(SingleMasterServer singleMasterServer, DmCommandParams commandParams) : base(commandParams)
        {
            _singleMasterServer = singleMasterServer;
        }

        public override async Task Execute(SocketMessage socketMessage, bool isRus, CommandAccessLevel accessLevel)
        {
            var args = socketMessage.CommandArgs();
            if (args.Length <= 0)
                return;
            var player = args[0];

            var info = GetPlayerInfo(player, accessLevel == CommandAccessLevel.Admin, isRus);
            await socketMessage.Channel.SendMessageAsync(info);
        }

        private string GetPlayerInfo(string nickName, bool isAdmin, bool isRus)
        {
            var player = _singleMasterServer.GetPlayer(nickName);
            if (player == null)
                return isRus ? "Этот игрок не зарегистрирован!" : "This player isn't registered!";
            
            var sb = new StringBuilder();

            if (isRus)
            {
                sb.AppendLine($"Ник: **{player.Name}**");
                sb.AppendLine($"Очки 1v1: **{player.Score1v1}**, 2v2: **{player.Score2v2}**, 3v3: **{player.Score3v3}**");
                sb.AppendLine(
                    $"Победы: **{player.WinsCount}**, Игры: **{player.GamesCount}**, Процент побед: **{Math.Round(player.WinRate * 100, 2)}%**, Макс. череда побед: **{player.Best1v1Winstreak}**");
                sb.AppendLine(
                    $"Кол-во игр: Космодесант: **{player.Smgamescount}**, Хаос: **{player.Csmgamescount}**, Орки: **{player.Orkgamescount}**, Эльдары: **{player.Eldargamescount}**, " +
                    $"Имперская гвардия: **{player.Iggamescount}**, Тау: **{player.Taugamescount}**, Некроны: **{player.Necrgamescount}**, Сестры битвы: **{player.Sobgamescount}**, Темные эльдары: **{player.Degamescount}**");
                sb.AppendLine($"Время проведенное в играх: **{Math.Round(player.AllInGameTicks / 60f / 60f, 1)}** часов");
                sb.AppendLine($"Профиль на dowstats: https://dowstats.ru/player.php?sid={player.Id}&server=elSpy");
                sb.AppendLine($"Привязанный стим: https://steamcommunity.com/profiles/{player.SteamId}");
            }
            else
            {
                sb.AppendLine($"Nickname: **{player.Name}**");
                sb.AppendLine($"MMR 1v1: **{player.Score1v1}**, 2v2: **{player.Score2v2}**, 3v3: **{player.Score3v3}**");
                sb.AppendLine(
                    $"Wins: **{player.WinsCount}**, Games: **{player.GamesCount}**, WinRate: **{Math.Round(player.WinRate * 100, 2)}%**, Max Winstreak: **{player.Best1v1Winstreak}**");
                sb.AppendLine(
                    $"Games count: Sm: **{player.Smgamescount}**, Csm: **{player.Csmgamescount}**, Orks: **{player.Orkgamescount}**, Eldar: **{player.Eldargamescount}**, " +
                    $"Ig: **{player.Iggamescount}**, Tau: **{player.Taugamescount}**, Necrons: **{player.Necrgamescount}**, Sob: **{player.Sobgamescount}**, De: **{player.Degamescount}**");
                sb.AppendLine($"Time spent in battles: **{Math.Round(player.AllInGameTicks / 60f / 60f, 1)}** hours");
                sb.AppendLine($"Dowstats profile: https://dowstats.ru/player.php?sid={player.Id}&server=elSpy");
                sb.AppendLine($"Steam profile: https://steamcommunity.com/profiles/{player.SteamId}");
            }

            if (isAdmin)
            {
                sb.AppendLine("--------------------------");
                sb.AppendLine($"Email: __**{player.Email}**__");
            }

            return sb.ToString();
        }
    }
}