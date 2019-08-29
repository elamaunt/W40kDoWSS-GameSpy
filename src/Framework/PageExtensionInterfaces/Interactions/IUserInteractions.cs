namespace Framework
{
    public interface IUserInteractions
    {
        INotification<NotificationResult> ShowErrorNotification(string message, string title = null, string acceptButtonTitle = null);
    }
}
