using System.Windows.Controls;

namespace Framework.WPF
{
    public interface IPageExtension : IExtension
    {
        void OnExtended(Page view, PageViewModel viewModel);
    }
}
