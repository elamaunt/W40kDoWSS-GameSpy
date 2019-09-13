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
            if (args.Count < 3)
                return;

            var hostname = args[0];
            var count = args[1];
            var type = args[2];
            var gamevariant = args[3];
            
            IrcDaemon.SendToAllGameBroadcast(count, type, gamevariant, hostname);
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
