using Discord.WebSocket;
using GSMasterServer.DiscordBot.Commands;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot
{
    internal class BotCommands
    {
        private static Dictionary<string, IBotCommand> commands = new Dictionary<string, IBotCommand>()
        {
            { "ping", new PingCommand() }
        };

        public static async Task HandleCommand(SocketMessage arg)
        {
            try
            {
                var commandParams = arg.Content.Split();
                var commandName = commandParams[0].Substring(1).ToLower();
                IBotCommand command;
                if (!commands.TryGetValue(commandName, out command))
                {
                    Logger.Trace($"Command: \"{commandName}\" is not implemented!");
                    return;
                }

                var userAccessLevel = AccessLevel.User;

                var author = arg.Author as SocketGuildUser;
                if (author.Roles.Any(x => x.Id == DiscordServerConstants.adminRoleId))
                    userAccessLevel = AccessLevel.Admin;
                else if (author.Roles.Any(x => x.Id == DiscordServerConstants.moderRoleId))
                    userAccessLevel = AccessLevel.Moderator;

                if (userAccessLevel >= command.MinAccessLevel)
                {
                    await commands[commandName].Execute(commandParams.Skip(1).ToArray(), arg);
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
    }
}
