namespace ThunderHawk.Core
{
    public interface IClientServer
    {
        void SendAsServerMessage(string message);

        void Start();
        void Stop();
    }
}
