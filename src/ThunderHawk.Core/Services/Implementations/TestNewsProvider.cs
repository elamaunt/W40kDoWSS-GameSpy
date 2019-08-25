using System;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    class TestNewsProvider : INewsProvider
    {
        public async Task<NewsItemDTO[]> GetNews()
        {
            await Task.Delay(2000).ConfigureAwait(false);

            return new NewsItemDTO[]
                {
                    new NewsItemDTO()
                    {
                        Author = "elamaunt",
                        RussianTitle = "Открытый бетатест сервера",
                        RussianAnnotation = "Этот день войдет в историю Soulstorm",
                        RussianText =  "<b>Дорогие любители Soulstorm!</b>\n\nМы объявляем начало новой эры, эры рейтинга, статистики и скила. Теперь все желающие могут играть в Warhammer 40k Soulstorm без многочисленных проблем Steam версии.",
                        ImagePath = "/Images/Primarismarines.jpg",
                        NewsTime = new DateTime(2019, 8, 23)
                    },

                    new NewsItemDTO()
                    {
                        Author = "elamaunt",
                        RussianTitle = "Гайд на орках от YbuBaKa",
                        RussianAnnotation = "Теперь все наконец-то научатся играть за орков",
                        RussianText = "<b>Здорова, Рубака!</b>\n\nЗаряди свою бигшуту и вперед смотреть <href>https://www.youtube.com/watch?v=eAZi9Ef1iY4</href>! После просмотра ты должен отчитаться на ладдере обо всем, что узнал, а также собрать для нас зубы. Вааааргх!",
                        ImagePath = "/Images/YbubakaGuide.png",
                        NewsTime = new DateTime(2019, 7, 30)
                    },

                    new NewsItemDTO()
                    {
                        Author = "elamaunt",
                        RussianTitle = "Найден лучший игрок в Soulstorm",
                        RussianAnnotation = "Вы не поверите, но лучшим игроком в Soulstorm теперь признан...",
                        RussianText = "Вы не поверите, но лучшим игроком в Soulstorm теперь признан...\nНикто! Конечно, ведь чтобы определить лучшего, нам нужен был ладдер. И теперь он есть. Статистика также теперь работает, а значит, вы можете бороться за звание короля автоподбора. Кто же займет 1 место в конце бетатеста, это мы скоро узнаем. А пока оцените наш лаунчер!\n\nВсегда с вами, elamaunt.",
                        ImagePath = "/Images/BestPlayer.png",
                        NewsTime = new DateTime(2019, 8, 23)
                    }
                };
        }
    }
}
