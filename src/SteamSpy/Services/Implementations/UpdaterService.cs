using System.Threading.Tasks;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class UpdaterService : IUpdaterService
    {
        public string CurrentVersion => throw new System.NotImplementedException();

        public string AvailableVersion => throw new System.NotImplementedException();

        public bool HasCriticalUpdate => throw new System.NotImplementedException();

        public Task<string> CheckForUpdates()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}
