using System.Collections.Generic;

namespace SharedServices
{
    public class EnMessagesService : MessagesServiceBase
    {
        protected override Dictionary<string, string> InflateValues()
        {
            return new Dictionary<string, string>()
            {
                [Messages.WELLCOME] = "Welcome to the server",
                [Messages.RATING_GAME] = "The rating changes only if the checkbox \"Rating game\" is checked.",
                [Messages.NOTIFICATIONS] = "When someone creates a game in automatch, you will receive an notification indicating the type and version.",
                [Messages.STATISTICS_CHANGES] = "Statistics are calculated on the latest version of the ThunderHawk mod when playing without computer-controlled players.",
                [Messages.PLAYERS_ON_SERVER] = "Total players on the server at the moment",
                [Messages.OPENED_GAMES_IN_AUTO] = "Open automatch games",
                [Messages.PLAYERS_IN_AUTO] = "number of players in a game search",
                [Messages.SOMEBODY_CREATED_A_GAME] = "Someone created a game in automatch",
                [Messages.RESTART_AUTOMATCH_ADVICE] = "If right now you start or restart automatch, then with a high probability you will immediately get into the game.",
                [Messages.RATING_CHANGED] = "Your rating {0} has changed on {1} and is now equal to {2}",
            };
        }
    }
}
