using IrcD.Channel;
using IrcD.Commands.Arguments;
using IrcD.Core;
using System.Collections.Generic;
using System.Linq;

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
                // elamaunt!Xv1sFqOa9X|17972147@192.168.1.31 UTM elamaunt :GML

                // This code only for warhammer

                var usersInGame = channel.Users.ToArray();

                IrcDaemon.RegisterRatingGame(usersInGame);

                for (int i = 0; i < usersInGame.Length; i++)
                {
                    var item = usersInGame[i];
                    item.WriteLine($@":{info.Usermask} UTM {args[0]} :{args[1]}");
                }
            }
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
