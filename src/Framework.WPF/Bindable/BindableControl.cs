using System.Windows;
using System.Windows.Controls;

namespace Framework.WPF
{
    public class BindableControl : UserControl, IBindableView
    {
        public ViewModel ViewModel
        {
            get => this.GetViewModel<ViewModel>();
            set => DataContext = value;
        }

        public BindableControl()
        {
            DataContextChanged += WPFBinder.OnDataContextChanged;
        }
    }
}
