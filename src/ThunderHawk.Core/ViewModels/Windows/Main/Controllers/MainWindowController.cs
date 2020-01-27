﻿using Framework;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    class MainWindowController : BindingController<IMainWindowView, MainWindowViewModel>
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

            CoreContext.OpenLogsService.LogMessageReceived += OnLogMessageReceived;

            OnUsersLoaded();

            if (AppSettings.IsFirstLaunch && !AppSettings.LaunchThunderHawkAtStartup)
            {
                Task.Delay(30000).OnContinueOnUi(() =>
                {
                    CoreContext.SystemService.AskUser("Would you like to add ThunderHawk at Windows startup? You can change it on Settings window in any time.").OnCompletedOnUi(result =>
                    {
                        AppSettings.LaunchThunderHawkAtStartup = result;
                    });

                    AppSettings.IsFirstLaunch = false;
                });
            }
        }

        void OnLogMessageReceived(string message)
        {
            RunOnUIThread(() =>
            {
                Frame.LogsViewModel.Messages.DataSource.Add(new LogMessageItemViewModel(message));
            });
        }

        void OnNewGameReceived(GameInfo game)
        {
            if (!CoreContext.MasterServer.IsLastGamesLoaded)
                return;

            var vm = new GameItemViewModel(game);

            RunOnUIThread(() =>
            {
                Frame.StatsViewModel.LastGames.DataSource.Insert(0, vm);
            });
        }

        void OnGameBroadcastReceived(GameHostInfo info)
        {
            if (info.IsUser)
            {
                CoreContext.SystemService.NotifyAsSystemToastMessage("Your host updated", $"{info.MaxPlayers / 2}vs{info.MaxPlayers / 2}, {info.Players}/{info.MaxPlayers}");

                RunOnUIThread(() =>
                {
                    Frame.ChatViewModel.Messages.DataSource.Add(new ChatMessageItemViewModel(new MessageInfo()
                    {
                        IsPrivate = true,
                        Text = $"Your host updated. {info.MaxPlayers / 2}vs{info.MaxPlayers / 2}, {info.Players}/{info.MaxPlayers}"
                    }));
                });
            }
            else
            {
                if (info.Ranked && (AppSettings.LimitRatingLobby || info.LimitedByRating))
                {
                    var profile = CoreContext.MasterServer.CurrentProfile;

                    long? userScore = null;

                    switch (info.MaxPlayers)
                    {
                        case 2:
                            userScore = profile.Score1v1;
                            break;
                        case 4:
                            userScore = profile.Score2v2;
                            break;
                        case 6:
                        case 8:
                            userScore = profile.Score3v3;
                            break;
                        default:
                            return;
                    }

                    if (userScore.HasValue && Math.Abs(info.Score - userScore.Value) > 180)
                        return;
                }

                CoreContext.SystemService.NotifyAsSystemToastMessage("Automatch host", $"GameVariant: {info.GameVariant}. GameType: {info.MaxPlayers / 2}vs{info.MaxPlayers / 2}. {info.Players}/{info.MaxPlayers}. Fixed teams: {info.Teamplay}. Ranked: {info.Ranked}");
                CoreContext.ClientServer.SendAsServerMessage($"Automatch host: {info.MaxPlayers / 2}vs{info.MaxPlayers / 2}, {info.GameVariant}.  {info.Players}/{info.MaxPlayers}. Fixed teams: {info.Teamplay}. Ranked: {info.Ranked}");

            }
        }

        void OnUserStatsChanged(StatsChangesInfo changes)
        {
            if (changes?.User?.IsUser ?? false)
            {
                string text;

                if (changes.Delta == 0)
                    text = $"Your games count has been changed.";
                else
                    text = $"Your rating{changes.GameType.ToString().Replace("_", " ")} has been changed on {GetDeltaStringValue(changes.Delta)} and now equals {Math.Max(1000, changes.CurrentScore)}.";

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

                Frame.ChatViewModel.ConnectedLabel.Text = $"Connection is active. Users on server {Frame.ChatViewModel.Users.ItemsCount}";
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
                Frame.ChatViewModel.ConnectedLabel.Text = $"Connection is active. Users on server {Frame.ChatViewModel.Users.ItemsCount}";
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
                CoreContext.InGameService.serverOnlinePlayers = collection; // share online users to this service to receive mmr there
                Frame.ChatViewModel.ConnectedLabel.Text = $"Connection is active. Users on server  {collection.Count}";
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

                if (info.Author?.IsUser ?? false)
                    return;

                if (View.IsActive && Frame.Pages.SelectedItem == Frame.ChatTabViewModel)
                    return;

                CoreContext.SystemService.NotifyAsSystemToastMessage(info);
            });
        }

        protected override void OnUnbind()
        {
            CoreContext.OpenLogsService.LogMessageReceived -= OnLogMessageReceived;
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
