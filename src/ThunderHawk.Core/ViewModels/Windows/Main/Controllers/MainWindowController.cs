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
            CoreContext.MasterServer.UserStatsChanged += OnUserStatsChanged;
            CoreContext.MasterServer.GameBroadcastReceived += OnGameBroadcastReceived;
            CoreContext.MasterServer.NewGameReceived += OnNewGameReceived;

            OnUsersLoaded();
        }

        void OnNewGameReceived(GameInfo game)
        {
            if (!CoreContext.MasterServer.IsLastGamesLoaded)
                return;

            var vm = new GameItemViewModel(game);

            RunOnUIThread(() =>
            {
                Frame.StatsViewModel.LastGames.DataSource.Add(vm);
            });
        }

        void OnGameBroadcastReceived(GameHostInfo info)
        {
            CoreContext.SystemService.NotifyAsSystemToastMessage("New automatch host", $"GameVariant: {info.GameVariant}. GameType: {info.MaxPlayers/2} vs {info.MaxPlayers / 2}. Teamplay: {info.Teamplay}");
            CoreContext.ClientServer.SendAsServerMessage($"New automatch host: {info.MaxPlayers / 2} vs {info.MaxPlayers / 2}, {info.GameVariant}. Teamplay: {info.Teamplay}");
        }

        void OnUserStatsChanged(StatsChangesInfo changes)
        {
            if (changes?.User?.IsUser ?? false)
            {
                var text = $"You rating {changes.GameType.ToString().Replace("_", " ")} has been changed on {GetDeltaStringValue(changes.Delta)} and now equals {changes.CurrentScore}.";

                RunOnUIThread(() =>
                {
                    CoreContext.ClientServer.SendAsServerMessage(text);

                    Frame.ChatViewModel.Messages.DataSource.Add(new ChatMessageItemViewModel(new MessageInfo()
                    {
                        IsPrivate = true,
                        Text = text
                    }));
                });
            }
        }

        string GetDeltaStringValue(long delta)
        {
            if (delta > 0)
                return "+" + delta;
            return delta.ToString();
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

                CoreContext.SystemService.NotifyAsSystemToastMessage(info);
            });
        }

        protected override void OnUnbind()
        {
            CoreContext.MasterServer.NewGameReceived -= OnNewGameReceived;
            CoreContext.MasterServer.ChatMessageReceived -= OnMessageReceived;
            CoreContext.MasterServer.UsersLoaded -= OnUsersLoaded;
            CoreContext.MasterServer.UserConnected -= OnUserConnected;
            CoreContext.MasterServer.UserDisconnected -= OnUserDisconnected;
            CoreContext.MasterServer.UserStatsChanged -= OnUserStatsChanged;
            CoreContext.MasterServer.GameBroadcastReceived -= OnGameBroadcastReceived;
            base.OnUnbind();
        }
    }
}
