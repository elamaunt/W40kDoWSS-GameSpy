using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.AdministrativeModule;
using DiscordBot.Commands.GeneralModule;
using DiscordBot.Commands.Primitives;
using DiscordBot.Database;

namespace DiscordBot.Commands
{
    internal class DmCommandsHandler
    {
        private readonly BotParams.BotParams _botParams;
        private readonly DowBot _dowBot;
        private readonly Dictionary<string, DmCommand> _commands = new Dictionary<string, DmCommand>();
        
        public DmCommand GetCommand(string key)
        {
            return _commands.ContainsKey(key) ? _commands[key] : null;
        }

        public IEnumerable<string> GetCommands(CommandAccessLevel authorAccess)
        {
            return _commands.Where(x => authorAccess >= x.Value.Params.AccessLevel ).Select(k => k.Key);
        }

        public DmCommandsHandler(DowBot dowBot, BotParams.BotParams botParams, GuildCommandsHandler guildCommandsHandler)
        {
            _botParams = botParams;
            _dowBot = dowBot;
            if (botParams.GeneralModuleParams.DmCommandsParams.ContainsKey(CommandId.HelpCommand))
                _commands.Add("help", new HelpCommand(botParams.GeneralModuleParams.DmCommandsParams[CommandId.HelpCommand], guildCommandsHandler, this));

            if (botParams.GeneralModuleParams.DmCommandsParams.ContainsKey(CommandId.SetLangCommand))
            {
                var slc = new SetLanguageCommand(botParams.GeneralModuleParams.DmCommandsParams[CommandId.SetLangCommand]);
                _commands.Add("sl", slc);
                _commands.Add("setlang", slc);
            }

            if (botParams.AdministrativeModuleParams != null)
            {
                if (botParams.AdministrativeModuleParams.DmCommandsParams.ContainsKey(CommandId.SendToAllCommand))
                {
                    _commands.Add("massnotify",
                        new SendToAllCommand(dowBot.AdminCommandsManager,
                            botParams.AdministrativeModuleParams.DmCommandsParams[CommandId.SendToAllCommand]));
                }
            }
            
            if (botParams.CustomCommandsModuleParams != null)
            {
                foreach (var item in botParams.CustomCommandsModuleParams.CustomDmCommands)
                    _commands.Add(item.Key, item.Value);
            }
        }
        
        public async Task HandleBotCommand(SocketMessage arg)
        {
            try
            {
                if (!(arg.Channel is SocketDMChannel))
                    return;
                
                var commandName = arg.Content.Split()[0].Substring(1).ToLower();
                if (!_commands.TryGetValue(commandName, out var command))
                {
                    return;
                }

                var userAccessLevel = await _botParams.GetAccessLevel(arg.Author, _dowBot.MainGuild);
                

                if (userAccessLevel >= command.Params.AccessLevel)
                {

                    
                    await _commands[commandName].Execute(arg, arg.Author.IsRussian(), userAccessLevel);
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