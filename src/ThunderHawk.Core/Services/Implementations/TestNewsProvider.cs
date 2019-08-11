using System;

namespace ThunderHawk.Core
{
    class TestNewsProvider : INewsProvider
    {
        public NewsItemDTO[] GetNews()
        {
            return new NewsItemDTO[]
                {
                    new NewsItemDTO()
                    {
                        RussianTitle = "Открытый бетатест сервера",
                        RussianAnnotation = "Дорогие любители Soulstorm! Мы объявляем начало новой эры, эры рейтинга, статистики и скила. теперь все желающие могут играть в Warhammer 40k Soulstorm без многочисленных проблем Steam версии.",
                        RussianText = "тест тесттесттесттесттесттесттесттест",
                        ImagePath = "/Images/Primarismarines.jpg",
                        NewsTime = DateTime.Now
                    },

                    new NewsItemDTO()
                    {
                        RussianTitle = "Гайд на орках от YbuBaKa",
                        RussianAnnotation = "Теперь все наконец-то научатся играть за орков",
                        RussianText = "тест тесттесттесттесттесттесттесттест",
                        ImagePath = "/Images/YbubakaGuide.png",
                        NewsTime = DateTime.Now.AddDays(-20)
                    },

                    new NewsItemDTO()
                    {
                        RussianTitle = "Найден лучший игрок в Soulstorm",
                        RussianAnnotation = "Вы не поверите, но лучшим игроком в Soulstorm теперь признан...",
                        RussianText = "тест тесттесттесттесттесттесттесттест",
                        ImagePath = "/Images/BestPlayer.png",
                        NewsTime = DateTime.Now.AddDays(-100)
                    }
                };
        }
    }
}
