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

        public static string RepName(int repCount)
        {
            var repModifier = CalculateReputation(repCount, true);
            switch (repCount)
            {
                case int _ when (repModifier >= 30): // 21 025
                    return "Baneblade";

                case int _ when (repModifier >= 25): // 14 400
                    return "Predator";

                case int _ when (repModifier >= 20): // 9 025
                    return "Defiler";

                case int _ when (repModifier >= 15): // 4 900
                    return "Raider";

                case int _ when (repModifier >= 10): // 2 025
                    return "Harlequin";

                case int _ when (repModifier >= 7): // 900
                    return "Celestian";

                case int _ when (repModifier >= 5): // 400
                    return "Raptor";

                case int _ when (repModifier >= 3): // 100
                    return "Fire Warrior";

                case int _ when (repModifier >= 2): // 25
                    return "Scout";

                default: // 0
                    return "Gretchin";
            }
        }


        public static int CalculateReputation(int changerRep, bool repAction)
        {
            var repChangeSignCoef = repAction ? 1 : -0.5f;
            if (changerRep < 0)
                changerRep = 0;

            var changerCoef = (int)(Math.Sqrt(changerRep) / 5);
            var repChange = (int)Math.Floor(repChangeSignCoef * (1 + changerCoef));

            if (repChange == 0)
                repChange = Math.Sign(repChangeSignCoef);
            return repChange;
        }
    }
}
