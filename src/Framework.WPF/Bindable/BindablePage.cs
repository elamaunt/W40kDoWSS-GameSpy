using System.Windows.Controls;

namespace Framework.WPF
{
    public partial class BindablePage : Page, IBindableView
    {
        public PageViewModel ViewModel
        {
            get => this.GetViewModel<PageViewModel>();
            set => DataContext = value;
        }
        ViewModel IBindableView.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PageViewModel)value;
        }

        public BindablePage()
        {
            DataContextChanged += WPFBinder.OnDataContextChanged;
        }
    }
}
