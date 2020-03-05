using System.Collections.Generic;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.BotParams
{
    public class AdministrativeModuleParams: IModuleParams, IModuleDmCommandsParams, IModuleGuildCommandsParams
    {
        public readonly ulong AdminRoleId;
        public readonly ulong ModerRoleId;
        public readonly ulong MutedRoleId;
        
        public Dictionary<CommandId, GuildCommandParams> GuildCommandsParams { get; } = new Dictionary<CommandId, GuildCommandParams>()
        {
            { CommandId.MuteCommand, new GuildCommandParams(CommandAccessLevel.Moderator, CommandUsageArea.Everywhere, CommandUsageAreaType.Allow) },
            { CommandId.UnMuteCommand, new GuildCommandParams(CommandAccessLevel.Moderator, CommandUsageArea.Everywhere, CommandUsageAreaType.Allow) },
            { CommandId.BanCommand, new GuildCommandParams(CommandAccessLevel.Admin, CommandUsageArea.Everywhere, CommandUsageAreaType.Allow) },
            { CommandId.UnBanCommand, new GuildCommandParams(CommandAccessLevel.Admin, CommandUsageArea.Everywhere, CommandUsageAreaType.Allow) },
            { CommandId.DeleteMessagesCommand, new GuildCommandParams(CommandAccessLevel.Admin, CommandUsageArea.Everywhere, CommandUsageAreaType.Allow) },
        };
        
        public Dictionary<CommandId, DmCommandParams> DmCommandsParams { get; } = new Dictionary<CommandId, DmCommandParams>()
        {
            { CommandId.SendToAllCommand, new DmCommandParams(CommandAccessLevel.Admin) }
        };

        public AdministrativeModuleParams(ulong adminRoleId, ulong moderRoleId, ulong mutedRoleId)
        {
            AdminRoleId = adminRoleId;
            ModerRoleId = moderRoleId;
            MutedRoleId = mutedRoleId;
        }
    }
}