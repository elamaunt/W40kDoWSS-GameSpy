using GSMasterServer.Data;
using System.Collections.Generic;
using System.Net;

namespace GSMasterServer.Services
{
    public interface IMainDataBase : IDataBase
    {
        bool TryRegisterGame(ref GameDBO game);

        bool AddFriend(long toProfileId, long friendProfileId);
        void RemoveFriend(long profileId, long friendProfileId);
        IEnumerable<ProfileDBO> GetProfilesBySteamId(long steamId);
        ProfileDBO GetProfileById(long profileId);
        ProfileDBO GetProfileByName(string username);
        void UpdateProfileData(ProfileDBO stats);
        
        List<ProfileDBO> GetAllProfilesBySteamId(ulong steamId);
        void LogProfileLogin(string name, ulong steamId, IPAddress address);

        ProfileDBO CreateProfile(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address);

        bool ProfileExists(string username);
        ProfileDBO[] Load1v1Top10();
        ProfileDBO[] LoadAllStats();

        Profile1X1DBO GetProfile1X1ByProfileId(long profileId);
        Profile2X2DBO GetProfile2X2ByProfileId(long profileId);
        Profile3X3DBO GetProfile3X3ByProfileId(long profileId);
        Profile4X4DBO GetProfile4X4ByProfileId(long profileId);
        Profile1X1DBO CreateProfile1X1(long profileId);
        Profile2X2DBO CreateProfile2X2(long profileId);
        Profile3X3DBO CreateProfile3X3(long profileId);
        Profile4X4DBO CreateProfile4X4(long profileId);
        void UpdateProfile1X1(Profile1X1DBO profile);
        void UpdateProfile2X2(Profile2X2DBO profile);
        void UpdateProfile3X3(Profile3X3DBO profile);
        void UpdateProfile4X4(Profile4X4DBO profile);
        GameDBO[] GetLastGames();
        void SaveLastActiveProfileForSteamId(ulong steamId, long profileId);
        long? GetLastActiveProfileForSteamId(ulong steamId);
    }
}