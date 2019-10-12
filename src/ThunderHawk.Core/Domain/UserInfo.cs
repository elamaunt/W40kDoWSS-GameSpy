using SharedServices;

namespace ThunderHawk.Core
{
    public class UserInfo
    {
        public readonly ulong SteamId;

        public string Status { get; set; }
        public string Name { get; set; }
        public long? ActiveProfileId { get; set; }

        public Race? Race { get; set; }
        public long? Games { get; set; }
        public long? Wins { get; set; }
        public long? Score1v1 { get; set; }
        public long? Score2v2 { get; set; }
        public long? Score3v3 { get; set; }
        public long? Best1v1Winstreak { get; set; }
        public string ClientUserName { get; set; }
        public string ClientBFlags { get; set; }

        public UserInfo(ulong steamId)
        {
            SteamId = steamId;
        }

        public string UIName => Name ?? CoreContext.SteamApi.GetUserName(SteamId) ?? SteamId.ToString();

    }
}
