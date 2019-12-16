namespace ThunderHawk.Core
{
    public interface IClientServer
    {
        string GetIndicator();
        void SendAsServerMessage(string message);

        void Start();
        void Stop();
    }
}
