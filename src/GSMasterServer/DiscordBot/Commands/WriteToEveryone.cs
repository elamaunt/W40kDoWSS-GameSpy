using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IrcNet.Tools;

namespace GSMasterServer.DiscordBot.Commands
{
    internal class WriteToEveryone : IBotDmCommand
    {
        private const string FilePath = "nicks.txt";

        public AccessLevel MinAccessLevel { get; } = AccessLevel.Admin;

        public async Task Execute(SocketMessage socketMessage, BotManager botManager, AccessLevel accessLevel)
        {
            var skipedText = socketMessage.Content.Split().Skip(1);
            var text = string.Join(" ", skipedText);

            var users = await botManager.Guild.GetUsersAsync();
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
            var first = true;
            foreach (var channel in channels)
            {
                if (first)
                {
                    first = false;
                    sb.Append(channel.Recipient.Username);
                }
                else
                {
                    sb.Append(", " + channel.Recipient.Username);
                }
            }

            using (var file = new StreamWriter(FilePath))
            {
                file.WriteLine(sb.ToString());
            }


            await socketMessage.Channel.SendFileAsync(FilePath, "Successfully sent everyone message to these users");
            File.Delete(FilePath);
        }
    }
}
