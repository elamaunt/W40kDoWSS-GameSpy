using System.Collections.Generic;

namespace SharedServices
{
    public static class Messages
    {
        public const string WELLCOME = nameof(WELLCOME);
        public const string RATING_GAME = nameof(RATING_GAME);
        public const string NOTIFICATIONS = nameof(NOTIFICATIONS);
        public const string STATISTICS_CHANGES = nameof(STATISTICS_CHANGES);
        public const string PLAYERS_ON_SERVER = nameof(PLAYERS_ON_SERVER);
        public const string OPENED_GAMES_IN_AUTO = nameof(OPENED_GAMES_IN_AUTO);
        public const string PLAYERS_IN_AUTO = nameof(PLAYERS_IN_AUTO);
        public const string SOMEBODY_CREATED_A_GAME = nameof(SOMEBODY_CREATED_A_GAME);
        public const string RESTART_AUTOMATCH_ADVICE = nameof(RESTART_AUTOMATCH_ADVICE);
        public const string RATING_CHANGED = nameof(RATING_CHANGED);
        
        static Dictionary<string, ICulturedMessagesService> _services = new Dictionary<string, ICulturedMessagesService>
            {
                ["en"] = new EnMessagesService(),
                ["ru"] = new RuMessagesService()
            };

        public static string Get(string key, string culture)
        {
            if (_services.TryGetValue(culture, out ICulturedMessagesService service))
                return service.Get(key);

            return _services["en"].Get(key);
        }
    }
}
