using Framework;
using System.Collections.ObjectModel;

namespace ThunderHawk.Core
{
    public class ChatPageController : FrameController<ChatPageViewModel>
    {
        protected override void OnBind()
        {
            CoreContext.MasterServer.ChatMessageReceived += OnMessageReceived;
            CoreContext.MasterServer.UsersLoaded += OnUsersLoaded;
            CoreContext.MasterServer.UserConnected += OnUserConnected;

            OnUsersLoaded();

            Frame.Send.Action = OnSendClicked;
        }

        void OnUserConnected(UserInfo info)
        {
            RunOnUIThread(() => Frame.Users.DataSource.Add(new ChatUserItemViewModel(info)));
        }

        void OnUsersLoaded()
        {
            var users = CoreContext.MasterServer.GetAllUsers();

            var collection = new ObservableCollection<ChatUserItemViewModel>();

            for (int i = 0; i < users.Length; i++)
                collection.Add(new ChatUserItemViewModel(users[i]));

            RunOnUIThread(() => Frame.Users.DataSource = collection);
        }

        void OnSendClicked()
        {
            var text = Frame.TextInput.Text;

            if (text.IsNullOrWhiteSpace())
                return;

            CoreContext.MasterServer.SendChatMessage(text);

            Frame.TextInput.Text = string.Empty;
        }

        void OnMessageReceived(MessageInfo info)
        {
            RunOnUIThread(() => Frame.Messages.DataSource.Add(new ChatMessageItemViewModel(info)));
        }

        protected override void OnUnbind()
        {
            CoreContext.MasterServer.ChatMessageReceived -= OnMessageReceived;
            CoreContext.MasterServer.UsersLoaded -= OnUsersLoaded;
            CoreContext.MasterServer.UserConnected -= OnUserConnected;
            base.OnUnbind();
        }
    }
}
