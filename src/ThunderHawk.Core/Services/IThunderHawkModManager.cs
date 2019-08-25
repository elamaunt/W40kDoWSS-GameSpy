using System.Threading;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface IThunderHawkModManager
    {
        string ModName { get; }
        bool CheckIsModExists(string gamePath);
        Task<bool> CheckIsLastVersion(string gamePath);
        Task DownloadMod(string gamePath, CancellationToken token);
        Task UpdateMod(string gamePath, CancellationToken token);
    }
}
