using System.Windows.Controls;

namespace Framework.WPF
{
    public interface ICustomContentPresenter
    {
        void Present(System.Windows.Controls.Frame frame, IBindableView content);
        void GoForward(System.Windows.Controls.Frame view);
        void GoBack(System.Windows.Controls.Frame view);
    }
}
