using System.Windows.Controls;

namespace Framework.WPF
{
    public class BindableTabItem : TabItem, IBindableView
    {
        public ViewModel ViewModel
        {
            get => this.GetViewModel<ViewModel>();
            set => DataContext = value;
        }

        public BindableTabItem()
        {
            DataContextChanged += WPFBinder.OnDataContextChanged;
        }

        ~BindableTabItem()
        {
            DataContextChanged -= WPFBinder.OnDataContextChanged;
        }
    }
}
