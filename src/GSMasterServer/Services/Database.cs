using GSMasterServer.Services;
using GSMasterServer.Services.Implementations;

namespace GSMasterServer
{
    public static class Database
    {
        public static readonly IMainDataBase MainDBInstance = new LiteDBMainDatabase();

        public static void Initialize(string databasePath)
        {
            if (!MainDBInstance.IsInitialized)
                MainDBInstance.Initialize(databasePath);
        }

        public static bool IsInitialized()
        {
            return MainDBInstance.IsInitialized;
        }
    }
}
