using System.Windows;

namespace Framework.WPF
{
    public partial class BindableWindow : Window, IBindableView
    {
        public WindowViewModel ViewModel
        {
            get => this.GetViewModel<WindowViewModel>();
            set => DataContext = value;
        }

        ViewModel IBindableView.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (WindowViewModel)value;
        }

        public BindableWindow()
        {
            DataContextChanged += WPFBinder.OnDataContextChanged;
        }
    }
}
