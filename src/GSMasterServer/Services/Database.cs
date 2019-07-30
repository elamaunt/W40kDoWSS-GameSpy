using GSMasterServer.Services;
using GSMasterServer.Services.Implementations;

namespace GSMasterServer
{
    public static class Database
    {
        //public static readonly IUsersDataBase UsersDBInstance = new SQLiteUsersDatabase();
        public static readonly IUsersDataBase UsersDBInstance = new LiteDBUsersDatabase();

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
