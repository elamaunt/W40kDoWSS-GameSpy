using System.Windows;

namespace Framework.WPF
{
    public interface ICustomItemPresenter
    {
        void Present(FrameworkElement parent, FrameworkElement cell);
    }
}
