using System.Windows;

namespace Framework.WPF
{
    public partial class BindableWindow : Window, IBindableWindow
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

        ~BindableWindow()
        {
            DataContextChanged -= WPFBinder.OnDataContextChanged;
        }
    }
}
