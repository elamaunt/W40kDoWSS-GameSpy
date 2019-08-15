using System;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public class ServerNewsProvider : INewsProvider
    {
        public Task<NewsItemDTO[]> GetNews()
        {
            throw new NotImplementedException();
        }
    }
}
