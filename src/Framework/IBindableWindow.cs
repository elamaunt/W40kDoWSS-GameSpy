using System.Threading.Tasks;

namespace Framework
{
    public interface IBindableWindow : IBindableView
    {
        Task CloseWaitingTask();
        void Close();
    }
}
