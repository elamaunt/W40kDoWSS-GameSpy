using GSMasterServer.Data;
using System.Collections.Generic;
using System.Net;

namespace GSMasterServer.Services
{
    public interface IMainDataBase : IDataBase
    {
        bool AddFriend(long toProfileId, long friendProfileId);
        void RemoveFriend(long profileId, long friendProfileId);
        ProfileDBO GetProfileById(long profileId);
        ProfileDBO GetProfileByName(string username);
        void UpdateProfileData(ProfileDBO stats);
        
        List<ProfileDBO> GetAllProfilesByEmailAndPass(string email, string passwordEncrypted);
        void LogProfileLogin(string name, ulong steamId, IPAddress address);

        ProfileDBO CreateProfile(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address);

        bool ProfileExists(string username);
        ProfileDBO[] Load1v1Top10();
        ProfileDBO[] LoadAllStats();

        NewsDBO[] GetLastNews(int count);
    }
}