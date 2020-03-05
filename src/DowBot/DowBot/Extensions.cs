using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.BotParams;
using DiscordBot.Commands;
using DiscordBot.Commands.Primitives;

namespace DiscordBot
{
    public static class Extensions
    {
        public static CommandAccessLevel GetAccessLevel(this BotParams.BotParams botParams, IUser user)
        {
            var socketUser = user as SocketGuildUser;
            if (socketUser != null && (socketUser.Roles.Any(x => x.Id == botParams.AdministrativeModuleParams.AdminRoleId) || socketUser.GuildPermissions.Administrator))
                return CommandAccessLevel.Admin;
            if (socketUser != null && socketUser.Roles.Any(x => x.Id == botParams.AdministrativeModuleParams.ModerRoleId))
                return CommandAccessLevel.Moderator;
            return CommandAccessLevel.Everyone;
        }
        
        public static async Task<CommandAccessLevel> GetAccessLevel(this BotParams.BotParams botParams, IUser user, IGuild guild)
        {
            var socketUser = await guild.GetUserAsync(user.Id) as SocketGuildUser;
            if (socketUser != null && (socketUser.Roles.Any(x => x.Id == botParams.AdministrativeModuleParams.AdminRoleId) || socketUser.GuildPermissions.Administrator))
                return CommandAccessLevel.Admin;
            if (socketUser != null && socketUser.Roles.Any(x => x.Id == botParams.AdministrativeModuleParams.ModerRoleId))
                return CommandAccessLevel.Moderator;
            return CommandAccessLevel.Everyone;
        }
        
        public static string[] CommandArgs(this SocketMessage arg)
        {
            return arg.Content.Split().Skip(1).ToArray();
        }
    }
}