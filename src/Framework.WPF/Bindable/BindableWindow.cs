using System;
using System.Threading.Tasks;
using System.Windows;

namespace Framework.WPF
{
    public partial class BindableWindow : Window, IBindableWindow
    {
        readonly TaskCompletionSource<SystemExtensionMethods.Void> _windowCloseCompletion = new TaskCompletionSource<SystemExtensionMethods.Void>();

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

        public Task CloseWaitingTask()
        {
            return _windowCloseCompletion.Task;
        }

        protected override void OnClosed(EventArgs e)
        {
            _windowCloseCompletion.TrySetResult(new SystemExtensionMethods.Void());
            base.OnClosed(e);
        }
    }
}
