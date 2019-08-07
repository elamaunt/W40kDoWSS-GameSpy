using IrcD.Commands.Arguments;
using IrcD.Core;
using System.Collections.Generic;

namespace IrcD.Commands
{
    public class RoomCounters : CommandBase
    {
        public RoomCounters(IrcDaemon ircDaemon)
            : base(ircDaemon, "ROOMCOUNTERS", "")
        {
        }

        protected override void PrivateHandle(UserInfo info, List<string> args)
        {
            IrcDaemon.Replies.SendMainRoomCounters(info);
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
