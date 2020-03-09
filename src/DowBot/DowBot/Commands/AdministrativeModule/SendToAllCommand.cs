using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.AdministrativeModule
{
    internal class SendToAllCommand: DmCommand
    {
        private readonly AdminCommandsManager _adminManager;

        public SendToAllCommand(AdminCommandsManager adminManager, DmCommandParams commandParams) : base(commandParams)
        {
            _adminManager = adminManager;
        }

        public override async Task Execute(SocketMessage socketMessage, bool isRus, CommandAccessLevel accessLevel)
        {
            var skipedText = socketMessage.Content.Split().Skip(1);
            var text = string.Join(" ", skipedText);
            var result = await _adminManager.SendMessageToEveryone(text);
            if (!result)
                return;
            
            await socketMessage.Channel.SendFileAsync(_adminManager.PathToTempNicks, "Successfully sent everyone message to these users");
            File.Delete(_adminManager.PathToTempNicks);
        }
    }
}