using GSMasterServer.Data;
using System;
using System.Collections.Generic;
using System.Net;

namespace GSMasterServer.Services
{
    public interface IProfilesDataBase : IDisposable
    {
        bool IsInitialized { get; }
        void Initialize(string databasePath);

        bool AddFriend(long toProfileId, long friendProfileId);
        void RemoveFriend(long profileId, long friendProfileId);
        ProfileData GetProfileById(long profileId);
        ProfileData GetProfileByName(string username);
        void UpdateProfileData(ProfileData stats);
        
        List<ProfileData> GetAllProfilesByEmailAndPass(string email, string passwordEncrypted);
        void LogProfileLogin(string name, ulong steamId, IPAddress address);

        ProfileData CreateProfile(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address);

        bool ProfileExists(string username);
        ProfileData[] Load1v1Top10();
        ProfileData[] LoadAllStats();
    }
}