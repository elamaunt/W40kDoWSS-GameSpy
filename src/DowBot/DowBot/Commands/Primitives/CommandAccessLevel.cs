namespace DiscordBot.Commands.Primitives
{
    public enum CommandAccessLevel: byte
    {
        Everyone = 0,
        Moderator = 1,
        Admin = 2
    }
}