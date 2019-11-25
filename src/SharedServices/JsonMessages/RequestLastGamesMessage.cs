namespace SharedServices
{
    public class RequestLastGamesMessage : Message
    {
        public RequestLastGamesMessage()
            : base(MessageTypes.RequestLastGames)
        {
        }

    }
}
