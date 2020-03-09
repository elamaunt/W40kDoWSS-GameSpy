namespace DiscordBot.Commands.Primitives
{
    public enum CommandId: ushort
    {
        HelpCommand = 0,
        SetLangCommand = 1,
        RandomCommand = 100,
        ShuffleCommand = 101,
        MuteCommand = 200,
        UnMuteCommand = 201,
        BanCommand = 202,
        UnBanCommand = 203,
        DeleteMessagesCommand = 204,
        SendToAllCommand = 205,
    }
}