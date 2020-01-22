﻿using Discord.WebSocket;
using GSMasterServer.DiscordBot.Commands;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot
{
    internal class BotCommandsManager
    {
        private readonly BotManager _botManager;
        public BotCommandsManager(BotManager botManager)
        {
            _botManager = botManager;
        }

        public string[] CommandStrings = {"!"};

        private readonly Dictionary<string, IBotCommand> _commands = new Dictionary<string, IBotCommand>()
        {
            { "dm", new DeleteMessagesCommand() },
            { "stm", new MuteCommand(true) },
            { "mute", new MuteCommand(false) },
            { "unstm", new UnMuteCommand(true) },
            { "unmute", new UnMuteCommand(false) },
            { "rr", new RandomCommand(false) },
            { "rm", new RandomCommand(true) }
        };

        private readonly Dictionary<string, IBotDmCommand> _dmCommands = new Dictionary<string, IBotDmCommand>()
        {
            { "everyone", new WriteToEveryone() },
            { "getp", new GetPlayerProfile() },
            { "ping", new PingCommand() }
        };

        public async Task HandleCommand(SocketMessage arg)
        {
            try
            {
                if (!(arg.Channel is SocketTextChannel channel))
                    return;

                var commandName = arg.Content.Split()[0].Substring(1).ToLower();
                if (!_commands.TryGetValue(commandName, out var command))
                {
                    //Logger.Trace($"Command: \"{commandName}\" is not implemented!");
                    return;
                }

                if (command.AllowLevel == AllowLevel.WholeCategory &&
                    (channel.CategoryId != DiscordServerConstants.RuChatsCategoryId && channel.CategoryId != DiscordServerConstants.EnChatsCategoryId))
                    return;

                if (command.AllowLevel == AllowLevel.OnlyChannel &&
                    channel.Id != DiscordServerConstants.BotChannelId)
                    return;



                var userAccessLevel = arg.Author.GetAccessLevel();



                if (userAccessLevel >= command.MinAccessLevel)
                {
                    await _commands[commandName].Execute(arg);
                    Logger.Trace($"Executed command \"{commandName}\" by {arg.Author.Username}({arg.Author.Id})" +
                        $" with Access Level {userAccessLevel}");
                }
                else
                {
                    Logger.Trace($"Failed to execute command \"{commandName}\" by {arg.Author.Username}({arg.Author.Id})" +
                        $" with Access Level \"{userAccessLevel}\", required \"{command.MinAccessLevel}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public async Task HandleDmCommand(SocketMessage arg)
        {
            try
            {
                var commandName = arg.Content.Split()[0].Substring(1).ToLower();
                if (!_dmCommands.TryGetValue(commandName, out var command))
                {
                    Logger.Trace($"DM Command: \"{commandName}\" is not implemented!");
                    return;
                }

                var userAccessLevel = await arg.Author.GetDmAccessLevel(_botManager.ThunderGuild);
                if (userAccessLevel >= command.MinAccessLevel)
                {
                    await _dmCommands[commandName].Execute(arg, _botManager, userAccessLevel);
                    Logger.Trace($"Executed DM command \"{commandName}\" by {arg.Author.Username}({arg.Author.Id})" +
                                 $" with Access Level {userAccessLevel}");
                }
                else
                {
                    Logger.Trace($"Failed to execute DM command \"{commandName}\" by {arg.Author.Username}({arg.Author.Id})" +
                                 $" with Access Level \"{userAccessLevel}\", required \"{command.MinAccessLevel}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

    }
}
