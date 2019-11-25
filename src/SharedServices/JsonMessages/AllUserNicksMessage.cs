namespace SharedServices
{
    public class AllUserNicksMessage : Message
    {
        public AllUserNicksMessage() 
            : base(MessageTypes.AllUserNicks)
        {
        }

        public string[] Nicks { get; set; }
    }
}
