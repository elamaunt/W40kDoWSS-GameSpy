using IrcD.Commands.Arguments;
using IrcD.Core;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class CDKey : CommandBase
    {
        public CDKey(IrcDaemon ircDaemon)
            : base(ircDaemon, "CDKey", "")
        { }

        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            IrcDaemon.Replies.SendCDKeyAuthentificated(info);
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
