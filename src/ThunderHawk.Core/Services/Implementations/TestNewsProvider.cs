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
                        RussianTitle = "Вышло обновление 6.2.3",
                        RussianAnnotation = "Короче сегодня. Да-да именно сегодня вышло новое крутое обновление! Бегом скачивать!",
                        RussianText = "тест тесттесттесттесттесттесттесттест",
                        ImagePath = "/Images/test-news-image.jpg",
                        NewsTime = DateTime.Now
                    },

                    new NewsItemDTO()
                    {
                        RussianTitle = "Турнир в честь великого Горка",
                        RussianAnnotation = "Эй ты, эльдаровод! Там аннонсировали новый турнир! Бегом регистрироваться!",
                        RussianText = "тест тесттесттесттесттесттесттесттест",
                        ImagePath = "/Images/test-news-image.jpg",
                        NewsTime = DateTime.Now
                    },

                    new NewsItemDTO()
                    {
                        RussianTitle = "Вышло обновление 6.2.2",
                        RussianAnnotation = "Короче сегодня. Да-да именно сегодня вышло новое крутое обновление! Бегом скачивать!",
                        RussianText = "тест тесттесттесттесттесттесттесттест",
                        ImagePath = "/Images/test-news-image.jpg",
                        NewsTime = DateTime.Now
                    }
                };
        }
    }
}
