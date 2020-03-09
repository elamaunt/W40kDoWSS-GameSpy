using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.BotParams;
using DiscordBot.Commands.AdministrativeModule;
using DiscordBot.Commands.Primitives;
using DiscordBot.Commands.RandomModule;
using DiscordBot.Database;
using RandomTools.Types;

namespace DiscordBot.Commands
{
    internal class GuildCommandsHandler
    {
        private readonly BotParams.BotParams _botParams;

        public GuildCommand GetCommand(string key)
        {
            return _commands.ContainsKey(key) ? _commands[key] : null;
        }
        
        public IEnumerable<string> GetCommands(CommandAccessLevel authorAccess)
        {
            return _commands.Where(x => authorAccess >= x.Value.Params.AccessLevel ).Select(k => k.Key);
        }
        
        private readonly Dictionary<string, GuildCommand> _commands = new Dictionary<string, GuildCommand>();
        
        public GuildCommandsHandler(DowBot dowBot, BotParams.BotParams botParams)
        {
            _botParams = botParams;
            
            if (botParams.RandomModuleParams != null)
            {
                if (botParams.RandomModuleParams.GuildCommandsParams.ContainsKey(CommandId.RandomCommand))
                {
                    _commands.Add("rr", new RandomCommand(DowItemType.Race, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.RandomCommand]));
                    var twoMapsRandomCommand = new RandomCommand(DowItemType.Map2p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.RandomCommand]);
                    _commands.Add("rm", twoMapsRandomCommand);
                    _commands.Add("rm2", twoMapsRandomCommand);
                    _commands.Add("rm4", new RandomCommand(DowItemType.Map4p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.RandomCommand]));  
                    _commands.Add("rm6", new RandomCommand(DowItemType.Map6p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.RandomCommand]));  
                    _commands.Add("rm8", new RandomCommand(DowItemType.Map8p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.RandomCommand]));  
                    _commands.Add("sr", new ShuffleCommand(DowItemType.Race, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.ShuffleCommand]));
                }

                if (botParams.RandomModuleParams.GuildCommandsParams.ContainsKey(CommandId.ShuffleCommand))
                {
                    var twoMapsShuffleCommand = new ShuffleCommand(DowItemType.Map2p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.ShuffleCommand]);
                    _commands.Add("sm", twoMapsShuffleCommand);
                    _commands.Add("sm2", twoMapsShuffleCommand);
                    _commands.Add("sm4", new ShuffleCommand(DowItemType.Map4p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.ShuffleCommand]));
                    _commands.Add("sm6", new ShuffleCommand(DowItemType.Map6p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.ShuffleCommand]));
                    _commands.Add("sm8", new ShuffleCommand(DowItemType.Map8p, botParams.RandomModuleParams.DowItemsProvider, botParams.RandomModuleParams
                        .GuildCommandsParams[CommandId.ShuffleCommand]));
                }
            }

            if (botParams.AdministrativeModuleParams != null)
            {
                if (botParams.AdministrativeModuleParams.GuildCommandsParams.ContainsKey(CommandId.MuteCommand))
                    _commands.Add("mute", new MuteCommand(dowBot.AdminCommandsManager, botParams.AdministrativeModuleParams.GuildCommandsParams[CommandId
                    .MuteCommand]));
                if (botParams.AdministrativeModuleParams.GuildCommandsParams.ContainsKey(CommandId.UnMuteCommand))
                    _commands.Add("unmute", new UnMuteCommand(dowBot.AdminCommandsManager, botParams.AdministrativeModuleParams.GuildCommandsParams[CommandId
                    .UnMuteCommand]));
                if (botParams.AdministrativeModuleParams.GuildCommandsParams.ContainsKey(CommandId.BanCommand))
                    _commands.Add("ban", new BanCommand(dowBot.AdminCommandsManager, botParams.AdministrativeModuleParams.GuildCommandsParams[CommandId.BanCommand]));
                if (botParams.AdministrativeModuleParams.GuildCommandsParams.ContainsKey(CommandId.DeleteMessagesCommand))
                    _commands.Add("dm", new DeleteMessagesCommand(dowBot.AdminCommandsManager, botParams.AdministrativeModuleParams.GuildCommandsParams[CommandId.DeleteMessagesCommand]));
                //_commands.Add("unban", new UnBanCommand(adminManager, botParams.AdministrativeModuleParams.GuildCommandsParams[CommandId.UnBanCommand]));
            }

            if (botParams.CustomCommandsModuleParams != null)
            {
                foreach (var item in botParams.CustomCommandsModuleParams.CustomGuildsCommands)
                    _commands.Add(item.Key, item.Value);
            }
        }
        
        public async Task HandleBotCommand(SocketMessage arg)
        {
            try
            {
                if (!(arg.Channel is SocketTextChannel channel))
                    return;
                var commandName = arg.Content.Split()[0].Substring(1).ToLower();
                if (!_commands.TryGetValue(commandName, out var command))
                {
                    return;
                }

                if (command.Params.UsageAreaType == CommandUsageAreaType.Allow)
                {
                    switch (command.Params.UsageArea)
                    {
                        case CommandUsageArea.Category when !command.Params.UsageIds.Contains(channel.Category.Id):
                        case CommandUsageArea.Channel when !command.Params.UsageIds.Contains(channel.Id):
                            return;
                    }
                }
                else // CommandUsageAreaType == Forbid
                {
                    switch (command.Params.UsageArea)
                    {
                        case CommandUsageArea.Everywhere:
                        case CommandUsageArea.Category when command.Params.UsageIds.Contains(channel.Category.Id):
                        case CommandUsageArea.Channel when command.Params.UsageIds.Contains(channel.Id):
                            return;
                    }
                }
                
                var userAccessLevel = _botParams.GetAccessLevel(arg.Author);

                if (userAccessLevel >= command.Params.AccessLevel)
                {
                    await _commands[commandName].Execute(arg,  arg.Author.IsRussian());
                    DowBotLogger.Trace($"Executed command \"{commandName}\" by {arg.Author.Username}({arg.Author.Id})" +
                                       $" with Access Level {userAccessLevel}");
                }
                else
                {
                    DowBotLogger.Trace($"Failed to execute command \"{commandName}\" by {arg.Author.Username}({arg.Author.Id})" +
                                       $" with Access Level \"{userAccessLevel}\", required \"{command.Params.AccessLevel}\"");
                }
            }
            catch (Exception e)
            {
                DowBotLogger.Warn("[DmCommandsHandler.cs - HandleBotCommand] An error happened while processing user command.\n" + e);
            }
        }
    }
}