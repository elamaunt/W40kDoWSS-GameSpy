using IrcD.Commands.Arguments;
using IrcD.Core;
using System;
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

            switch (count)
            {
                case "2":
                    IrcDaemon.SendToAll($"[{DateTime.UtcNow.ToString("hh:mm")}] Кто-то создал игру в автоматчинге  1 на 1 ({type}). Если прямо сейчас вы запустите или перезапустите автоподбор, то с большой вероятностью вы сразу же попадете в игру.", hostname);
                    break;
                case "4":
                    IrcDaemon.SendToAll($"[{DateTime.UtcNow.ToString("hh:mm")}] Кто-то создал игру в автоматчинге  2 на 2 ({type}). Если прямо сейчас вы запустите или перезапустите автоподбор, то с большой вероятностью вы сразу же попадете в игру.", hostname);
                    break;
                case "6":
                    IrcDaemon.SendToAll($"[{DateTime.UtcNow.ToString("hh:mm")}] Кто-то создал игру в автоматчинге  3 на 3 ({type}). Если прямо сейчас вы запустите или перезапустите автоподбор, то с большой вероятностью вы сразу же попадете в игру.", hostname);
                    break;
                case "8":
                    IrcDaemon.SendToAll($"[{DateTime.UtcNow.ToString("hh:mm")}] Кто-то создал игру в автоматчинге  4 на 4 ({type}). Если прямо сейчас вы запустите или перезапустите автоподбор, то с большой вероятностью вы сразу же попадете в игру.", hostname);
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
