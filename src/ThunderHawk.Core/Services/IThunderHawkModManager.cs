using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface IThunderHawkModManager
    {
        string ActiveModRevision { get; }
        string ModName { get; }
        bool CheckIsModExists(string gamePath);
        Task DownloadMod(string gamePath, CancellationToken token, Action<float> progressReporter);
        Task UpdateMod(string gamePath, CancellationToken token, Action<float> progressReporter);
    }
}
