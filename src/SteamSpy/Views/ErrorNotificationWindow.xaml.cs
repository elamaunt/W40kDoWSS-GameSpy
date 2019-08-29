using Framework;
using Framework.WPF;
using System.Threading.Tasks;

namespace ThunderHawk
{
    public partial class ErrorNotificationWindow : BindableWindow, INotification<NotificationResult>
    {
        readonly TaskCompletionSource<NotificationResult> _tcs = new TaskCompletionSource<NotificationResult>();

        public ErrorNotificationWindow(string message, string title, string acceptButtonTitle)
        {
            InitializeComponent();

            var vm = new ErrorWindowViewModel();

            vm.Title.Text = title;
            vm.Message.Text = message;
            vm.AcceptButton.Text = acceptButtonTitle ?? "Ok";

            ViewModel = vm;
        }

        public Task<NotificationResult> AwaitResult()
        {
            return _tcs.Task;
        }

        class ErrorWindowViewModel : WindowViewModel
        {
            public TextFrame Message { get; } = new TextFrame();
            public ButtonFrame AcceptButton { get; } = new ButtonFrame();
        }
    }
}
