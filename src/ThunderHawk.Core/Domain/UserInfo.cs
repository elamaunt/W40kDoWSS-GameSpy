namespace ThunderHawk.Core
{
    public class UserInfo
    {
        public readonly ulong SteamId;

        public string Status { get; set; }
        public string Name { get; set; }
        public long? ActiveProfileId { get; set; }

        public UserInfo(ulong steamId)
        {
            SteamId = steamId;
        }

        public string UIName => Name ?? CoreContext.SteamApi.GetUserName(SteamId) ?? SteamId.ToString();

    }
}
