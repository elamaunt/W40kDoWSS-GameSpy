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

        public override async Task Execute(SocketMessage socketMessage, CommandAccessLevel accessLevel)
        {
            var args = socketMessage.CommandArgs();
            if (args.Length <= 0)
                return;
            var player = args[0];

            var info = GetPlayerInfo(player, accessLevel == CommandAccessLevel.Admin);
            await socketMessage.Channel.SendMessageAsync(info);
        }

        private string GetPlayerInfo(string nickName, bool isAdmin)
        {
            var player = _singleMasterServer.GetPlayer(nickName);
            if (player == null)
                return "This player isn't registered!";
            
            var sb = new StringBuilder();
            sb.AppendLine($"Nickname: **{player.Name}**");
            sb.AppendLine($"MMR 1v1: **{player.Score1v1}**, 2v2: **{player.Score2v2}**, 3v3: **{player.Score3v3}**");
            sb.AppendLine(
                $"Wins: **{player.WinsCount}**, Games: **{player.GamesCount}** **({Math.Round(player.WinRate * 100, 2)}%)**, Winstreak: **{player.Best1v1Winstreak}**");
            sb.AppendLine(
                $"Games count: Sm: **{player.Smgamescount}**, Csm: **{player.Csmgamescount}**, Orks: **{player.Orkgamescount}**, Eldar: **{player.Eldargamescount}**, " +
                $"Ig: **{player.Iggamescount}**, Tau: **{player.Taugamescount}**, Necrons: **{player.Necrgamescount}**, Sob: **{player.Sobgamescount}**, De: **{player.Degamescount}**");
            //sb.AppendLine($"Favourite race: **{GetRaceName(player.FavouriteRace)}**");
            sb.AppendLine($"Time spent in battles: **{Math.Round(player.AllInGameTicks / 60f / 60f, 1)}** hours");
            sb.AppendLine($"Dowstats profile: https://dowstats.ru/player.php?sid={player.Id}&server=elSpy");
            if (isAdmin)
            {
                sb.AppendLine("--------------------------");
                sb.AppendLine($"Email: __**{player.Email}**__");
                sb.AppendLine($"Steam profile: __https://steamcommunity.com/profiles/{player.SteamId}__");
            }

            return sb.ToString();
        }
    }
}