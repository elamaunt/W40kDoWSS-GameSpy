using IrcD.Commands.Arguments;
using IrcD.Core;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class GameBroadcast : CommandBase
    {
        public GameBroadcast(IrcDaemon ircDaemon)
            : base(ircDaemon, "GAMEBROADCAST", "")
        {
        }

        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            //IrcDaemon.Replies.SendCDKeyAuthentificated(info);
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
