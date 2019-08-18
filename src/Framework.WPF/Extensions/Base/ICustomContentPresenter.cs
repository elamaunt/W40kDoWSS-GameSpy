using System.Windows.Controls;

namespace Framework.WPF
{
    public interface ICustomContentPresenter
    {
        void Present(System.Windows.Controls.Frame frame, IBindableView content);
    }
}
