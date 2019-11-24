namespace ThunderHawk.Core
{
    public interface IThunderHawkModManager
    {
        string ModName { get; }
        string ModVersion { get; }
        void DeployModAndModule(string gamePath);
    }
}
