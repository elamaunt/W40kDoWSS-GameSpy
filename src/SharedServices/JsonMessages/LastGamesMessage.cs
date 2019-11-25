namespace SharedServices
{
    public class LastGamesMessage : Message
    {
        public LastGamesMessage()
            : base(MessageTypes.LastGames)
        {
        }

        public GameFinishedMessage[] Games { get; set; }
    }
}
