using Framework;
using Framework.WPF;

namespace ThunderHawk
{
    public class UserInteractionsExtension : IUserInteractions, IControlExtension
    {
        BindableControl _control;
        public void OnExtended(BindableControl view)
        {
            _control = view;
        }


        public INotification<NotificationResult> ShowErrorNotification(string message, string title = null, string acceptButtonTitle = null)
        {
            return new ErrorNotificationWindow(message, title, acceptButtonTitle);
        }

        public void CleanUp()
        {
        }
    }
}
