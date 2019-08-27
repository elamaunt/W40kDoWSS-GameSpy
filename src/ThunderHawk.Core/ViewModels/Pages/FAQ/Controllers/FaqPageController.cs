﻿using Framework;
using System.Collections.ObjectModel;

namespace ThunderHawk.Core
{
    public class FaqPageController : FrameController<FAQPageViewModel>
    {
        protected override void OnBind()
        {
            Frame.Questions.DataSource = new ObservableCollection<QuestionItemViewModel>()
            {
                new QuestionItemViewModel("Для чего нужен этот лаунчер?","Этот лаунчер позволяет играть в версию Soulstorm 1.2 используя Steam соединение между игроками и подключение к нашему серверу для работы чата и статистики. При этом для игры используется старая версия 1.2, для которой выполняется эмуляция GameSpy"),
                new QuestionItemViewModel("Могу ли я играть на пиратской версии игры?", "Нет, для игры через лаунчер ThunderHawk необходима установленная версия игры, купленная в Steam. Игра будет запускаться через специальный патч для перехода на версию 1.2."),
                new QuestionItemViewModel("Исправлены ли баги официальной версии игры?", "Исправлены пока только баги, никак не связанные с балансом игры. Для игры используется специальный мод, который со временем будет улучшаться. Изначально в этот мод уже вредрен другой мод Bugfix версии 1.56a. Багов Steam версии в версии 1.2 нет."),
                new QuestionItemViewModel("Будет ли меняться баланс в игре?", "Да, над балансом со временем начнется работа. Сперва важно наладить стабильность системы."),
                new QuestionItemViewModel("Появится ли внутриигровой чат в лаунчере?", "Да, чат обязательно появится. И не только чат ;)"),
                new QuestionItemViewModel("Что делает кнопка \"Установить\"?", "Эта кнопка скачивает последнюю версию мода, который необходим для игры с другими игроками. Мод будет скачан в папку, где установлен лаунчер. Для установки требуется объем памяти около 600МБ (это значение увеличится с обновлениями). Лаунчер автоматически при запуске настроит игру для мода."),
                new QuestionItemViewModel("Поддерживаются ли другие моды?", "Другие моды поддерживаются, Вы сможете играть на них на сервере, но эти игры никак не повлияют на вашу статистику."),
                new QuestionItemViewModel("Почему моя статистика не меняется?", "Статистика не учитывается при игре не через официальный мод или если в игре был хотя бы один компьютерный игрок."),
                new QuestionItemViewModel("Когда закончится бетатест?", "Как только сервер и лаунчер станут более стабильными для игры. Трудно сказать сейчас, когда это наступит."),
                new QuestionItemViewModel("Могу ли как-то поддержать проект?", @"Вы очень поможете проекту, если расскажите о нем другим людям. Если Вы хотите поддержать проект финансово, то Вы можете использовать следующие реквитизы:

PayPal по моей почте elamaunt@gmail.com
Киви: https://qiwi.com/p/79126193069
Яндекс: https://money.yandex.ru/to/410015861462468

Сбербанк на карту по номеру телефона 89126193069. Номер карты 4276160021263166
Сбербанк по счету:
Получатель: ДМИТРИЙ СЕРГЕЕВИЧ С.
Номер счёта: 40817810816542741233
Банк получателя: УРАЛЬСКИЙ БАНК ПАО СБЕРБАНК
БИК: 046577674
Корр. счёт: 30101810500000000674
ИНН: 7707083893
КПП: 667143001
SWIFT-код: SABRRUMM"),

                new QuestionItemViewModel("Как выполняется подсчет очков в игре в автоподборе?", "Расчет идет по формуле ELO с коэффициентом 32."),
                new QuestionItemViewModel("Почему я попался сильному игроку при игре в автоподборе?", "В данный момент система фильтрации игроков по очкам временно отключена. Это значит, что вам попадется случайный игрок со случайным рейтингом."),
                new QuestionItemViewModel("Почему я постоянно долго вижу слово \"автоподбор\"?", "Это слово может висеть, если Ваша игра пытается установить соединение с другим игроком. Если Вы видите его больше 20 секунд, то мы рекомендуем перезапустить поиск."),
                new QuestionItemViewModel("Почему мы с другом не соединяемся в автоподборе?", "В игру встроена система, не позволяющая подключиться друг к другу игрокам, которые нажали на поиск примерно в одно время."),
                new QuestionItemViewModel("Почему у меня лагает игра?", "Скорее всего лаги связаны с вашим соединением с сетью интернет или со скоростью Вашего компьютера."),
                new QuestionItemViewModel("Как работает автоподбор?", "При нажатии на поиск, игра пытается найти подходящий хост. Если хост найден, то Вы скорее всего сразу начнете играть. Если нет, то Вы сами станете хостом, и другие игроки смогут к Вам подключиться через некоторое время."),
                new QuestionItemViewModel("Как создать аккаунт в игре?", "Для создания аккаунта необходимо использовать форму прямо в игре при нажатии на мультиплеер в меню."),
                new QuestionItemViewModel("Почему при создании аккаунта у меня показывается ошибка соединения?", @"Ошибка соединения может быть по нескольким причинам: 
1) Ваш ник мог быть уже занят другим пользователем. Попробуйте воспользоваться другим ником.
2) У Вас действительно нет соединения в сервером или сервер на данный момент приостановлен.
3) Возможно у Вас некорректно прописан CD ключ игры в реестре. CD ключ необходим для входа в игру, так как это требовалось для GameSpy. CD ключ может быть нелицензионный. Эту проблему можно исправить при помощи разблокировки рас в разделе ""модификации""."),

            };
        }
    }
}
