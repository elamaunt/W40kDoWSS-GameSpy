using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.BotParams;
using DiscordBot.Commands.Primitives;
using DiscordBot.Database;

namespace DiscordBot.Commands.AdministrativeModule
{
    internal class AdminCommandsManager
    {
        private readonly BotParams.BotParams _botParams;
        
        private SocketRole _muteRole;
        private SocketGuild _mainGuild;

        public AdminCommandsManager(DowBot dowBot, BotParams.BotParams botParams)
        {
            _botParams = botParams;
            dowBot.OnBotReady += OnDowBotReady;
        }

        private void OnDowBotReady(object sender, SocketGuild e)
        {
            _muteRole = e.GetRole(_botParams.AdministrativeModuleParams.MutedRoleId);
            _mainGuild = e;
        }

        public async Task<List<SocketUser>> MuteAsync(IReadOnlyCollection<SocketUser> users, long muteUntil)
        {
            var mutedUsers = new List<SocketUser>();
            foreach (var user in users)
            {
                if (_botParams.GetAccessLevel(user) >= CommandAccessLevel.Moderator) continue;
                
                BotDatabase.AddMute(user.Id, muteUntil);
                mutedUsers.Add(user);
                
                if (user is SocketGuildUser guildUser)
                    await guildUser.AddRoleAsync(_muteRole);
            }

            return mutedUsers;
        }
        
        public async Task<List<SocketUser>> UnMuteAsync(IReadOnlyCollection<SocketUser> users)
        {
            var unMutedUsers = new List<SocketUser>();
            foreach (var user in users)
            {
                if (_botParams.GetAccessLevel(user) >= CommandAccessLevel.Moderator) continue;
                
                BotDatabase.RemoveMute(user.Id);
                unMutedUsers.Add(user);
                
                if (user is SocketGuildUser guildUser)
                    await guildUser.RemoveRoleAsync(_muteRole);
            }

            return unMutedUsers;
        }

        public async Task<List<SocketUser>> BanAsync(IReadOnlyCollection<SocketUser> users, sbyte days)
        {
            var bannedUsers = new List<SocketUser>();
            foreach (var user in users)
            {
                if (_botParams.GetAccessLevel(user) >= CommandAccessLevel.Moderator) continue;
                await _mainGuild.AddBanAsync(user, days);
                bannedUsers.Add(user);
            }

            return bannedUsers;
        }
        
        public async Task<List<SocketUser>> UnBanAsync(IReadOnlyCollection<SocketUser> users)
        {
            var unBannedUsers = new List<SocketUser>();
            foreach (var user in users)
            {
                if (_botParams.GetAccessLevel(user) >= CommandAccessLevel.Moderator) continue;
                
                await _mainGuild.RemoveBanAsync(user);
                unBannedUsers.Add(user);
            }

            return unBannedUsers;
        }

        public readonly string PathToTempNicks = "nicks.txt";
        public async Task<bool> SendMessageToEveryone(string text)
        {
            var users = await (_mainGuild as IGuild).GetUsersAsync();
            var channelTasks = new List<Task<IDMChannel>>();
            foreach (var user in users)
            {
                try
                {
                    if (user.IsBot)
                        continue;

                    channelTasks.Add(user.GetOrCreateDMChannelAsync());
                }
                catch (Exception ex)
                {
                    DowBotLogger.Warn(ex);
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
                    DowBotLogger.Warn(ex);
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

            using (var file = new StreamWriter(PathToTempNicks))
            {
                file.WriteLine(sb.ToString());
            }

            return true;
        }
    }
}