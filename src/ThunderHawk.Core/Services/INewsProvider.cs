using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface INewsProvider
    {
        Task<NewsItemDTO[]> GetNews();
    }
}
