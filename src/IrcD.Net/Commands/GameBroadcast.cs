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
            if (args.Count < 2)
                return;

            var hostname = args[0];
            var count = args[1];

            switch (count)
            {
                case "2":
                    IrcDaemon.SendToAll("Кто-то создал игру в автоматчинге  1 на 1", hostname);
                    break;
                case "4":
                    IrcDaemon.SendToAll("Кто-то создал игру в автоматчинге  2 на 2", hostname);
                    break;
                case "6":
                    IrcDaemon.SendToAll("Кто-то создал игру в автоматчинге  3 на 3", hostname);
                    break;
                case "8":
                    IrcDaemon.SendToAll("Кто-то создал игру в автоматчинге:  4 на 4", hostname);
                    break;
                default:
                    break;
            }
        }

        protected override int PrivateSend(CommandArgument argument)
        {
            throw new System.NotImplementedException();
        }
    }
}
