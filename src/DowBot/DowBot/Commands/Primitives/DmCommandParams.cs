namespace DiscordBot.Commands.Primitives
{
    /// <summary>
    /// User can manually specify these parameters!
    /// </summary>
    public class DmCommandParams
    {
        public readonly CommandAccessLevel AccessLevel;
        
        public DmCommandParams(CommandAccessLevel accessLevel)
        {
            AccessLevel = accessLevel;
        }
    }
}