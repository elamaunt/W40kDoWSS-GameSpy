using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot
{
    public static class BotExtensions
    {
        public static AccessLevel GetAccessLevel(this IUser user)
        {
            var socketUser = user as SocketGuildUser;
            if (socketUser != null && (socketUser.Roles.Any(x => x.Id == DiscordServerConstants.AdminRoleId) || socketUser.GuildPermissions.Administrator))
                return AccessLevel.Admin;
            if (socketUser != null && socketUser.Roles.Any(x => x.Id == DiscordServerConstants.ModerRoleId))
                return AccessLevel.Moderator;
            return AccessLevel.User;
        }

        public static async Task<AccessLevel> GetDmAccessLevel(this IUser user, IGuild thunderGuild)
        {
            var socketUser = await thunderGuild.GetUserAsync(user.Id) as SocketGuildUser;
            if (socketUser != null && (socketUser.Roles.Any(x => x.Id == DiscordServerConstants.AdminRoleId) || socketUser.GuildPermissions.Administrator))
                return AccessLevel.Admin;
            if (socketUser != null && socketUser.Roles.Any(x => x.Id == DiscordServerConstants.ModerRoleId))
                return AccessLevel.Moderator;
            return AccessLevel.User;
        }


        public static string[] CommandArgs(this SocketMessage arg)
        {
            return arg.Content.Split().Skip(1).ToArray();
        }
    }
}
