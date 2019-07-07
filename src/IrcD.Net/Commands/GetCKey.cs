using IrcD.Channel;
using IrcD.Commands.Arguments;
using IrcD.Core;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class GetCKey : CommandBase
    {
        public GetCKey(IrcDaemon ircDaemon) 
            : base(ircDaemon, "GETCKEY", "")
        {
        }

        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            // GETCKEY #GPG!2 * 003 0 :\b_stats

            if (IrcDaemon.Channels.TryGetValue(args[0], out ChannelInfo channel))
            {
                if (args[4] == "\\username\\b_flags")
                {
                    IrcDaemon.Replies.SendAllUsersFlags(info, channel, args);
                }

                if (args[4] == "\\b_stats")
                {
                    IrcDaemon.Replies.SendUsersStats(info, channel, args);
                }
            }
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
