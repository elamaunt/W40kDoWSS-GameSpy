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
        private static readonly Dictionary<string, IBotCommand> commands = new Dictionary<string, IBotCommand>()
        {
            { "ping", new PingCommand() },
            { "dm", new DeleteMessagesCommand() },
            { "sm", new MuteCommand(true) },
            { "mute", new MuteCommand(false) },
            { "unsm", new UnMuteCommand(true) },
            { "unmute", new UnMuteCommand(false) },
        };

        public static async Task HandleCommand(SocketMessage arg)
        {
            try
            {
                var commandName = arg.Content.Split()[0].Substring(1).ToLower();
                if (!commands.TryGetValue(commandName, out IBotCommand command))
                {
                    Logger.Trace($"Command: \"{commandName}\" is not implemented!");
                    return;
                }

                var userAccessLevel = arg.Author.GetAccessLevel();

                if (userAccessLevel >= command.MinAccessLevel)
                {
                    await commands[commandName].Execute(arg);
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
