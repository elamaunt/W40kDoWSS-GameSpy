using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IrcNet.Tools;

namespace GSMasterServer.DiscordBot.Commands
{
    public class WriteToEveryone : IBotDmCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Admin;

        public async Task Execute(SocketMessage socketMessage, IGuild thunderGuild)
        {
            var skipedText = socketMessage.Content.Split().Skip(1);
            var text = string.Join("", skipedText);

            var users = await thunderGuild.GetUsersAsync();
            var channelTasks = new List<Task<IDMChannel>>();
            foreach (var user in users)
            {
                try
                {
                    if (user.IsBot || user.Id == socketMessage.Author.Id)
                        continue;

                    channelTasks.Add(user.GetOrCreateDMChannelAsync());
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            }

            var channels = await Task.WhenAll(channelTasks);
            var messageTasks = new List<Task<IUserMessage>>();
            foreach (var channel in channels)
            {
                try
                {
                    messageTasks.Add(channel.SendMessageAsync(text));
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            }
            await Task.WhenAll(messageTasks);

            var sb = new StringBuilder();
            sb.AppendLine("Успешно отослали сообщение ВСЕМ пользователям сервера ThunderHawk!");
            sb.AppendLine("Список пользователей, которым было отослано сообщение:");
            foreach (var channel in channels)
            {
                sb.AppendLine(channel.Recipient.Username);
            }

            await socketMessage.Channel.SendMessageAsync(sb.ToString());
        }
    }
}
