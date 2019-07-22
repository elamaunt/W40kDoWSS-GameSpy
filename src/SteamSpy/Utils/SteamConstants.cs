using Steamworks;

namespace SteamSpy.Utils
{
    internal static class SteamConstants
    {
        /*public const long GLOBAL_CHAT_ID0 = 110338190901408189;
        public const long GLOBAL_CHAT_ID1 = 110338190895758280;
        public const long GLOBAL_CHAT_ID2 = 110338190901547414;
        public const long GLOBAL_CHAT_ID3 = 110338190901547529;*/

        public const uint PRODUCTION_APP_ID = 685420;

        public const EServerMode AUTENTIFICATION_MODE = EServerMode.eServerModeAuthenticationAndSecure;
        public const ushort STEAM_PORT = 8766;
        public const ushort SPECTATOR_PORT = 27017;

        public const ushort GAME_PORT = 27015;
        public const ushort QUERY_PORT = 27016;
        public const string INDICATOR = "SteamSpyW40k_" + GameConstants.VERSION;
    }
}