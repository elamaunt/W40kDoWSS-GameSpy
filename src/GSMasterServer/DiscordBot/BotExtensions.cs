using Discord;
using Discord.WebSocket;
using IrcNet.Tools;
using System.Linq;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot
{
    public static class BotExtensions
    {
        public static AccessLevel GetAccessLevel(this IUser user)
        {
            var socketUser = user as SocketGuildUser;
            if (socketUser.Roles.Any(x => x.Id == DiscordServerConstants.adminRoleId))
                return AccessLevel.Admin;
            else if (socketUser.Roles.Any(x => x.Id == DiscordServerConstants.moderRoleId))
                return AccessLevel.Moderator;
            return AccessLevel.User;
        }

        public static string[] CommandArgs(this SocketMessage arg)
        {
            return arg.Content.Split().Skip(1).ToArray();
        }
    }
}
