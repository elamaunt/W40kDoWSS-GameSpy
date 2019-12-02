using SharedServices;
using System.Threading;

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
        public long? Average { get; set; }
        public long? Disconnects { get; set; }

        volatile int _indexCounter = 1;

        public bool IsUser { get; }

        public string BStats { get; set; }
        public string BFlags { get; set; }

        public UserInfo(ulong steamId, bool isUser)
        {
            IsUser = isUser;
            SteamId = steamId;
        }

        public string UIName => Name ?? PrepareSteamName(CoreContext.SteamApi.GetUserName(SteamId)) ?? SteamId.ToString();

        public bool IsProfileActive => ActiveProfileId.HasValue && Name != null;

        static string PrepareSteamName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return name
                .Replace(" ", "_")
                .Replace("!", "_")
                .Replace("|", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_")
                .Replace("@", "_")
                .Replace("*", "_");
        }
    }
}
