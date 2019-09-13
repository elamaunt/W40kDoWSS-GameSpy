using ApiDomain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    class TestNewsProvider : INewsProvider
    {
        public async Task<NewsItemDTO[]> LoadLastNews(CancellationToken token)
        {
            await Task.Delay(2000).ConfigureAwait(false);

            return new NewsItemDTO[]
                {
                    new NewsItemDTO()
                    {
                        Author = "elamaunt",

                        Russian = new NewsLanguageItemDTO()
                        {
                            Title = "Финал Soulstorm Map Contest 2019",
                            Annotation = "Финал конкурса наступил и теперь судьям предстоит выбрать лучшие карты, которые имеют все шансы быть в автоподборе в ближайшем будущем!",
                            Body =  "Финал конкурса наступил и теперь судьям предстоит выбрать лучшие карты, которые имеют все шансы быть в автоподборе в ближайшем будущем!\n\nВы можете также ознакомиться со всеми картами участников самостоятельно и проголосовать за приз зрительских симпатий по ссылке <href>http://forums.warforge.ru/index.php?showtopic=258595</href>. Победитель будет объявлен 15 Сентября, следите за новостями.",
                        },

                        English = new NewsLanguageItemDTO()
                        {
                            Title = "Soulstorm Map Contest 2019 final",
                            Annotation = "The final competition has come and now the judges have to choose the best maps that have every chance of being in automatch in the near future!",
                            Body =  "The final competition has come and now the judges have to choose the best maps that have every chance of being in automatch in the near future!\n\nYou can also familiarize yourself with all the maps of the participants yourself and vote for the community prize at the link <href>http://forums.warforge.ru/index.php?showtopic=258595</href>. The winner will be announced on September 15th, so stay tuned.",
                        },

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/MapContest.png",
                        CreatedDate = new DateTime(2019, 9, 12)
                    },
                    new NewsItemDTO()
                    {
                        Author = "elamaunt",

                        Russian = new NewsLanguageItemDTO()
                        {
                            Title = "Открытый бетатест сервера",
                            Annotation = "Этот день войдет в историю Soulstorm",
                            Body =  "<b>Дорогие любители Soulstorm!</b>\n\nЯ объявляю начало новой эры, эры рейтинга, статистики и скилла. Теперь все желающие могут играть в Warhammer 40k Soulstorm без многочисленных проблем Steam версии. Многое еще необходимо сделать, но с сегодняшнего дня я постараюсь поддерживать сервер в активном состоянии. Сражайтесь, набирайте рейтинг! Возможно, в конце бетатеста будут организованы призы для лучших игроков рейтинговой таблицы ;)",
                        },

                        English = new NewsLanguageItemDTO()
                        {
                            Title = "Open server beta test",
                            Annotation = "This day will go down in Soulstorm history.",
                            Body =  "<b>Dear Soulstorm lovers!</b>\n\nI am announcing the start of a new era, an era of ranking, statistics and skill. Now everyone can play the Warhammer 40k Soulstorm without numerous Steam problems. Much more needs to be done, but from today I will try to keep the server active. Fight, gain rating! Perhaps at the end of the beta test prizes will be organized for the best players in the ranking table ;)",
                        },

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/Primarismarines.jpg",
                        CreatedDate = new DateTime(2019, 9, 7)
                    },

                    new NewsItemDTO()
                    {
                        Author = "elamaunt",

                        Russian = new NewsLanguageItemDTO()
                        {
                            Title = "Гайд на орках от YbuBaKa",
                            Annotation = "Теперь все наконец-то научатся играть за орков",
                            Body = "<b>Здорова, Рубака!</b>\n\nЗаряди свою бигшуту и вперед смотреть <href>https://www.youtube.com/watch?v=eAZi9Ef1iY4</href>! После просмотра ты должен отчитаться на ладдере обо всем, что узнал, а также собрать для нас зубы. Вааааргх!",
                        },

                        English = new NewsLanguageItemDTO()
                        {
                            Title = "Orc Guide by YbuBaKa",
                            Annotation = "Now everyone will finally learn how to play as orks",
                            Body = "<b>Hello, Grunt!</b>\n\nCharge your bigshoot and look ahead <href>https://www.youtube.com/watch?v=eAZi9Ef1iY4</href>! After watching, you should report on the ladder about everything that you learned, and also collect teeth for us. Waaaargh!",
                        },

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/YbubakaGuide.png",
                        CreatedDate = new DateTime(2019, 7, 30)
                    }
                };
        }
    }
}
