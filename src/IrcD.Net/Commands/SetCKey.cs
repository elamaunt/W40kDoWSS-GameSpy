using IrcD.Channel;
using IrcD.Commands.Arguments;
using IrcD.Core;
using System.Collections.Generic;

namespace IrcD.Commands
{
    class SetCKey : CommandBase
    {
        public SetCKey(IrcDaemon ircDaemon)
            : base(ircDaemon, "SETCKEY", "")
        {
        }

        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            if (IrcDaemon.Channels.TryGetValue(args[0], out ChannelInfo channel))
            {
                if (args[2].StartsWith(@"\b_flags"))
                {
                    info.UserFlags = args[2].Substring(9);
                }

                if (args[2].StartsWith(@"\b_stats"))
                {
                    info.UserStats = args[2].Substring(9);
                }


                IrcDaemon.Replies.SendSetKeyBroadcast(info, channel, args);
            }

            // "\\b_stats\\87654321|1000|0|"

            //:s 702 Shinto #gsp!!test CHC 0 :\XFOWvpDvpX|0\\7557fac37091291343e9b28c0eb8e59f
            //:s 702 sF|elamaunt #GPG!2 003 0  X44vuulsFX|15597086

            //IrcDaemon.Replies.SendStatsBroadcast();

            /*
            :s 702 #GSP!redalert3pcb!MaPJ9aPhaM #GSP!redalert3pcb!MaPJ9aPhaM Sidonuke BCAST :\b_flags\s
            >EncodedIP/ProfileID/Flags List
            :s 702 Sidonuke #GSP!redalert3pcb!MaPJ9aPhaM Sidonuke 023 :\XlG1W4OFpX|153849803\s
            >Notice he has 'sh' flags? it means hes staging and a host
            :s 702 Sidonuke #GSP!redalert3pcb!MaPJ9aPhaM car2nr 023 :\X44vuulsFX|155970863\sh
            :s 703 Sidonuke #GSP!redalert3pcb!MaPJ9aPhaM 023 :End of GETCKEY
            >More game Lobby Settings
            >Pings to be used for NAT Nego
            :car2nr!*@* UTM #GSP!redalert3pcb!MaPJ9aPhaM :Pings/ ,,0,0,0,0
            >PIDs... Unknown
            :car2nr!*@* UTM #GSP!redalert3pcb!MaPJ9aPhaM :PIDS/ 0, ,92b8fcb, , , , , , , , , ,
            >Ranking Update from us
            :s 702 #GSP!redalert3pcb!MaPJ9aPhaM #GSP!redalert3pcb!MaPJ9aPhaM Sidonuke BCAST :\b_clanName\\b_arenaTeamID\0\b_locale\0\b_wins\62\b_losses\32\b_rank1v1\\b_rank2v2\\b_clan1v1\\b_clan2v2\\b_onlineRank\0
            >Ranking info for users
            :s 702 Sidonuke #GSP!redalert3pcb!MaPJ9aPhaM Sidonuke 024 :\\0\0\62\32\\\\\0
            :s 702 Sidonuke #GSP!redalert3pcb!MaPJ9aPhaM car2nr 024 :\\0\0\0\5\\\\\0
            :s 703 Sidonuke #GSP!redalert3pcb!MaPJ9aPhaM 024 :End of GETCKEY
            */

        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
