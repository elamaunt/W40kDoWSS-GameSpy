using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface IUpdaterService
    {
        string CurrentVersionUI { get; }
        string CurrentVersion { get; }
        string NewestVersion { get; }

        event Action<string> NewVersionAvailable;

        void Update();

        bool CanUpdate { get; }

        Task<bool> CheckForUpdates();
        IComparer<string> VersionComparer { get; }
    }
}
