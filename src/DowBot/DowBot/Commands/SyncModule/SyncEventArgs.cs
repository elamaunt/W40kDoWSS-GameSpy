using System;

namespace DiscordBot.Commands.SyncModule
{
    public class SyncEventArgs: EventArgs
    {
        public string Author { get; }
        public string Text { get; }

        public SyncEventArgs(string author, string text)
        {
            Author = author;
            Text = text;
        }
    }
}