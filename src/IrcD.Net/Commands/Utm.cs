using IrcD.Channel;
using IrcD.Commands.Arguments;
using IrcD.Core;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class Utm : CommandBase
    {
        public Utm(IrcDaemon ircDaemon)
            : base(ircDaemon, "UTM", "")
        {
        }

        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            // "#GSP!whammer40kdc!MJD13lhaPM"
            // "GML "

            // list(channel.members)[i].message(":" + self.prefix + " UTM " + arguments[0] + " :" + arguments[1])
            
            if (IrcDaemon.Channels.TryGetValue(args[0], out ChannelInfo channel))
            {
                foreach (var item in channel.Users)
                    item.WriteLine($@":s UTM {args[0]} :{args[1]}");
            }
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
