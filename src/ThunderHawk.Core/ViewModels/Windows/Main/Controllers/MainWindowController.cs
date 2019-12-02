using Framework;
using System;
using System.Collections.ObjectModel;

namespace ThunderHawk.Core
{
    class MainWindowController : FrameController<MainWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.OpenSettings.Action = OpenSettings;
           
            var nickText = CoreContext.SteamApi.NickName;
            if (nickText == "")
                nickText = CoreContext.LangService.GetString("SteamNotLaunched");
            Frame.UserAccount.Text = nickText;

            CoreContext.MasterServer.ChatMessageReceived += OnMessageReceived;
            CoreContext.MasterServer.UsersLoaded += OnUsersLoaded;
            CoreContext.MasterServer.UserConnected += OnUserConnected;
            CoreContext.MasterServer.UserDisconnected += OnUserDisconnected;

            OnUsersLoaded();
        }

        void OnUserDisconnected(UserInfo info)
        {
            RunOnUIThread(() =>
            {
                var ds = Frame.ChatViewModel.Users.DataSource;

                for (int i = 0; i < ds.Count; i++)
                {
                    var item = ds[i];

                    if (item.Info.SteamId == info.SteamId)
                    {
                        ds.Remove(item);
                        i--;
                    }
                }

                Frame.ChatViewModel.ConnectedLabel.Text = $"Соединение активно. Пользователей на сервере {Frame.ChatViewModel.Users.ItemsCount}";
            });
        }

        void OpenSettings()
        {
            Frame.GlobalNavigationManager.OpenWindow<SettingsWindowViewModel>();
        }

        void OnUserConnected(UserInfo info)
        {
            RunOnUIThread(() =>
            {
                Frame.ChatViewModel.Users.DataSource.Add(new ChatUserItemViewModel(info));
                Frame.ChatViewModel.ConnectedLabel.Text = $"Соединение активно. Пользователей на сервере {Frame.ChatViewModel.Users.ItemsCount}";
            });
        }

        void OnUsersLoaded()
        {
            var users = CoreContext.MasterServer.GetAllUsers();

            var collection = new ObservableCollection<ChatUserItemViewModel>();

            for (int i = 0; i < users.Length; i++)
                collection.Add(new ChatUserItemViewModel(users[i]));

            RunOnUIThread(() =>
            {
                Frame.ChatViewModel.Users.DataSource = collection;
                Frame.ChatViewModel.ConnectedLabel.Text = $"Соединение активно. Пользователей на сервере {collection.Count}";
            });
        }

        void OnMessageReceived(MessageInfo info)
        {
            RunOnUIThread(() =>
            {
                if (Frame.Pages.SelectedItem != Frame.ChatTabViewModel)
                {
                    Frame.ChatTabViewModel.NewMessagesCounter.Value++;
                    Frame.ChatTabViewModel.NewMessagesCounter.Visible = true;
                }

                var newItem = new ChatMessageItemViewModel(info);

                Frame.ChatViewModel.Messages.DataSource.Add(newItem);
                Frame.ChatViewModel.MessagesScrollManager?.ScrollToItem(newItem);

                CoreContext.SystemService.NotifyAboutMessage(info);
            });
        }

        protected override void OnUnbind()
        {
            CoreContext.MasterServer.ChatMessageReceived -= OnMessageReceived;
            CoreContext.MasterServer.UsersLoaded -= OnUsersLoaded;
            CoreContext.MasterServer.UserConnected -= OnUserConnected;
            CoreContext.MasterServer.UserDisconnected -= OnUserDisconnected;
            base.OnUnbind();
        }
    }
}
