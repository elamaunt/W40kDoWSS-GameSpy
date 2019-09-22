using System.Collections.Generic;

namespace SharedServices
{
    public class EnMessagesService : MessagesServiceBase
    {
        protected override Dictionary<string, string> InflateValues()
        {
            return new Dictionary<string, string>()
            {
                [LangMessages.WELLCOME] = "Welcome to the server",
                [LangMessages.RATING_GAME] = "The rating changes only if the checkbox \"Rating game\" is checked.",
                [LangMessages.NOTIFICATIONS] = "When someone creates a game in automatch, you will receive an notification indicating the type and version.",
                [LangMessages.STATISTICS_CHANGES] = "Statistics are calculated on the latest version of the ThunderHawk mod when playing without computer-controlled players.",
                [LangMessages.PLAYERS_ON_SERVER] = "Total players on the server at the moment",
                [LangMessages.OPENED_GAMES_IN_AUTO] = "Open automatch games",
                [LangMessages.PLAYERS_IN_AUTO] = "number of players in a game search",
                [LangMessages.SOMEBODY_CREATED_A_GAME] = "Someone created a game in automatch",
                [LangMessages.RESTART_AUTOMATCH_ADVICE] = "If right now you start or restart automatch, then with a high probability you will immediately get into the game.",
                [LangMessages.RATING_CHANGED] = "Your rating {0} has changed on {1} and is now equal to {2}",
            };
        }
    }
}
