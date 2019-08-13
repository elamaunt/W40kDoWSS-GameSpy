using System.Threading.Tasks;

namespace Framework
{
    public interface INotification<ResultType>
    {
        Task<ResultType> AwaitResult();
        void Close();
    }
}
