using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface IUpdaterService
    {
        string CurrentVersion { get; }
        string AvailableVersion { get; }
        bool HasCriticalUpdate { get; }

        void Update();
        Task<string> CheckForUpdates();
    }
}
