using System;
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
            if (socketUser != null && (socketUser.Roles.Any(x => x.Id == DiscordServerConstants.adminRoleId) || socketUser.GuildPermissions.Administrator))
                return AccessLevel.Admin;
            if (socketUser != null && socketUser.Roles.Any(x => x.Id == DiscordServerConstants.moderRoleId))
                return AccessLevel.Moderator;
            return AccessLevel.User;
        }

        public static string[] CommandArgs(this SocketMessage arg)
        {
            return arg.Content.Split().Skip(1).ToArray();
        }

        public static string RepName(int repCount)
        {
            switch (repCount)
            {
                case int _ when (repCount >= 10000):
                    return "TITAN";
                case int _ when (repCount >= 5000):
                    return "Baneblade";
                case int _ when (repCount >= 2500):
                    return "Dreadnought";
                case int _ when (repCount >= 1000):
                    return "Defiler";
                case int _ when (repCount >= 500):
                    return "Force Commander";
                case int _ when (repCount >= 300):
                    return "Obliterator";
                case int _ when (repCount >= 100):
                    return "Warp Spider";
                case int _ when (repCount >= 50):
                    return "Fire Warrior";
                case int _ when (repCount >= 25):
                    return "Shoota Boy";
                case int _ when (repCount >= 10):
                    return "Guardian";
                default:
                    return "Gretchin";
            }
        }

        public static int CalculateReputation(int changerRep, bool repAction)
        {
            var repChangeSign = repAction ? 3 : -2;
            var repChange = (int)((5 + Math.Sqrt(changerRep)) * repChangeSign / 10);
            return repChange;
        }
    }
}
