using ApiDomain;
using System.Threading;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface INewsProvider
    {
        Task<NewsItemDTO[]> LoadLastNews(CancellationToken token);
    }
}
