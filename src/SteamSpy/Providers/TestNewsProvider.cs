using Common;
using System;

namespace SteamSpy.Providers
{
    class TestNewsProvider : INewsProvider
    {
        private bool getOnlyOnce = false;
        public News[] GetNews()
        {
            if (getOnlyOnce)
                return null;
            getOnlyOnce = true;
            var tNews1 = new News("Вышло обновление 6.2.3", "Короче сегодня. Да-да именно сегодня вышло новое крутое обновление! Бегом скачивать!", "тест тесттесттесттесттесттесттесттест",
                "test", "test test test", "test test test test test", NewsType.Update, 
                DateTime.UtcNow.Ticks - 10000, DateTime.UtcNow.Ticks - 10000, 1);
            var tNews2 = new News("Турнир в честь великого Горка", "Эй ты, эльдаровод! Там аннонсировали новый турнир! Бегом регистрироваться!", "тест тес2ттесттест2есттесттесттесттест",
                "test", "test test test", "test test test test test", NewsType.Update,
                DateTime.UtcNow.Ticks, DateTime.UtcNow.Ticks, 2);
            return new News[] { tNews1, tNews2, tNews2, tNews1, tNews2 };
        }
    }
}
