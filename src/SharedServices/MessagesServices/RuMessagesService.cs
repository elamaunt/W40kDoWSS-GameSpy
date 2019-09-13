using System.Collections.Generic;

namespace SharedServices
{
    public class RuMessagesService : MessagesServiceBase
    {
        protected override Dictionary<string, string> InflateValues()
        {
            return new Dictionary<string, string>()
            {
                [Messages.WELLCOME] = "Добро пожаловать на сервер",
                [Messages.RATING_GAME] = "Рейтинг меняется, только в том случае, если стоит галочка \"Игра на счет\".",
                [Messages.NOTIFICATIONS] = "Когда кто-то создаст игру в автоподборе, вам придет оповещение с указанием типа и версии.",
                [Messages.STATISTICS_CHANGES] = "Статистика начисляется на последней версии ThunderHawk мода при игре без игроков, управляемых компьютером.",
                [Messages.PLAYERS_ON_SERVER] = "Всего игроков на сервере в данный момент",
                [Messages.OPENED_GAMES_IN_AUTO] = "Открытых игр в авто",
                [Messages.PLAYERS_IN_AUTO] = "количество игроков в поиске игры",
                [Messages.SOMEBODY_CREATED_A_GAME] = "Кто-то создал игру в автоматчинге",
                [Messages.RESTART_AUTOMATCH_ADVICE] = "Если прямо сейчас вы запустите или перезапустите автоподбор, то с большой вероятностью вы сразу же попадете в игру.",
                [Messages.RATING_CHANGED] = "Ваш рейтинг {0} изменился на {1} и сейчас равен {2}",
            }; 
        }
    }
}
