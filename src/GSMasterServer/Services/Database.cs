using GSMasterServer.Services;
using GSMasterServer.Services.Implementations;

namespace GSMasterServer
{
    public static class Database
    {
        //public static readonly IUsersDataBase UsersDBInstance = new SQLiteUsersDatabase();
        public static readonly IProfilesDataBase UsersDBInstance = new LiteDBProfilesDatabase();

        public static void Initialize(string databasePath)
        {
            if (!UsersDBInstance.IsInitialized)
                UsersDBInstance.Initialize(databasePath);
        }

        public static bool IsInitialized()
        {
            return UsersDBInstance.IsInitialized;
        }
    }
}
