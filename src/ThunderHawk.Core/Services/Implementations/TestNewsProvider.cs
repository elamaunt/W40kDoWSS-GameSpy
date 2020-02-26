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
                        Author = "Anibus",

                        Russian = new NewsLanguageItemDTO
                        {
                            Title = "ThunderHawk 2.10",
                            Annotation = "Изменения в проекте",
                            Body =  "<b>Дорогие игроки!</b>\n\n По многочисленным просьбам мы вернули кнопку запуска стим версии и упростили первоначальную установку, а также пофиксили часть багов. Удачи в рейтинге!\n" +
                                    "- sugar_oasis(2)\n" +
                                    "- deadly_fun_archeology(2)\n"+
                                    "- cold_war(4)\n" +
                                    "удалена карта: edemus gamble(2)"
                        },

                        English = new NewsLanguageItemDTO
                        {
                            Title = "ThunderHawk 2.10",
                            Annotation = "Global changes",
                            Body =  "<b>Dear Players!</b>\n\n Add new tab InGame, now you can see random player's races, mmr and loading status! Added three new maps in automatch:\n" +
                                    "- sugar_oasis(2)\n" +
                                    "- deadly_fun_archeology(2)\n"+
                                    "- cold_war(4)\n" + 
                            "map remove: edemus gamble(2)"
                        },

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/InGame.jpg",
                        CreatedDate = new DateTime(2020, 02, 28)
                    },
                    
                     new NewsItemDTO()
                    {
                        Author = "elamaunt",

                        Russian = new NewsLanguageItemDTO()
                        {
                            Title = "ThunderHawk 2.0",
                            Annotation = "Неужели, дождались?",
                            Body =  "<b>Дорогие любители Soulstorm!</b>\n\n   Прошло много времени с прошлого бетатеста, и теперь вы можете полноценно насладиться обновленной версией сервера. Месяцы кропотливой работы дали свои плоды. Множество проблем позади и теперь каждый может (неважно, лицензия или пиратка) играть вместе друг с другом в автоматче. Пока сервер временно позволяет играть только с использованием мода ThunderHawk. В будущем будет поддержка и других модов. Все зависит от вашей поддержки, мы вас точно не подведем =). А сейчас пора снова выяснить, кто лучший. \nУспехов на ладдере!",
                        },

                        English = new NewsLanguageItemDTO()
                        {
                            Title = "ThunderHawk 2.0",
                            Annotation = "Really, have you waited?",
                            Body =  "<b>Dear Soulstorm lovers!</b>\n\n   A lot of time has passed since the last beta test, and now you can fully enjoy the updated version of the server. Months of hard work have borne fruit. A lot of problems are behind and now everyone can (whether it be a license or a pirate) play automatch together with each other. While the server temporarily allows you to play only using the ThunderHawk mod. In the future there will be support for other mods. It all depends on your support, we definitely won’t let you down =). Now it's time to find out again who is the best. \nSuccess on ladder!",
                        },

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/ThunderHawk2.png",
                        CreatedDate = new DateTime(2019, 12, 21)
                    },

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

                    /*new NewsItemDTO()
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
                    }*/
                };
        }
    }
}
