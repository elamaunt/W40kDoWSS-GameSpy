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
                            Title = "Открытый бетатест сервера",
                            Annotation = "Этот день войдет в историю Soulstorm",
                            Body =  "<b>Дорогие любители Soulstorm!</b>\n\nМы объявляем начало новой эры, эры рейтинга, статистики и скила. Теперь все желающие могут играть в Warhammer 40k Soulstorm без многочисленных проблем Steam версии.",
                        },

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/Primarismarines.jpg",
                        CreatedDate = new DateTime(2019, 8, 23)
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

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/YbubakaGuide.png",
                        CreatedDate = new DateTime(2019, 7, 30)
                    },

                    new NewsItemDTO()
                    {
                        Author = "elamaunt",

                        Russian = new NewsLanguageItemDTO()
                        {
                            Title = "Найден лучший игрок в Soulstorm",
                            Annotation = "Вы не поверите, но лучшим игроком в Soulstorm теперь признан...",
                            Body = "Вы не поверите, но лучшим игроком в Soulstorm теперь признан...\nНикто! Конечно, ведь чтобы определить лучшего, нам нужен был ладдер. И теперь он есть. Статистика также теперь работает, а значит, вы можете бороться за звание короля автоподбора. Кто же займет 1 место в конце бетатеста, это мы скоро узнаем. А пока оцените наш лаунчер!\n\nВсегда с вами, elamaunt.",
                        },

                        ImagePath = "pack://application:,,,/ThunderHawk;component/Images/BestPlayer.png",
                        CreatedDate = new DateTime(2019, 8, 23)
                    }
                };
        }
    }
}
