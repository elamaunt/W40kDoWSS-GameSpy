using Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace ThunderHawk.Core
{
    public class LobbiesController : FrameController<ChatPageViewModel>
    {
        volatile int _lastUpdateTime;
        Timer _timer;

        protected override void OnBind()
        {
            CoreContext.MasterServer.GameBroadcastReceived += OnBroadcastReceived;
            CoreContext.ClientServer.LobbiesUpdatedByRequest += OnLobbiesUpdatedByRequest;

            _timer = new Timer(OnTimerReceived, null, 5000, 5000);

            ReloadLobbies();
        }

        void OnTimerReceived(object state)
        {
            UpdateLobbiesIfNeeded();
        }

        private void OnLobbiesUpdatedByRequest(GameHostInfo[] hosts)
        {
            var source = ToSource(hosts);

            RunOnUIThread(() =>
            {
                Frame.GamesInAuto.DataSource = source;
            });
        }

        void OnBroadcastReceived(GameHostInfo host)
        {
            UpdateLobbiesIfNeeded();
        }

        void UpdateLobbiesIfNeeded()
        {
            if ((Environment.TickCount - _lastUpdateTime) > 1000)
            {
                Thread.MemoryBarrier();
                _lastUpdateTime = Environment.TickCount;
                ReloadLobbies();
            }
        }

        void ReloadLobbies()
        {
            CoreContext.SteamApi.LoadLobbies()
                .ContinueWith(t => ToSource(t.Result))
                .OnCompletedOnUi(models => Frame.GamesInAuto.DataSource = models);
        }

        private ObservableCollection<ItemViewModel> ToSource(GameHostInfo[] hosts)
        {
            var source = new ObservableCollection<ItemViewModel>();

            int maxPlayers = 0;

            foreach (var host in hosts.OrderBy(x => x.MaxPlayers).OrderBy(x => x.Players))
            {
                if (!host.Ranked)
                    continue;

                if (maxPlayers != host.MaxPlayers)
                {
                    maxPlayers = host.MaxPlayers;

                    var separator = new GamesSeparatorItemViewModel();
                    separator.Header.Text = $"{maxPlayers / 2}vs{maxPlayers / 2}";
                    source.Add(separator);
                }

                source.Add(new GameLobbyItemViewModel(host));
            }

            return source;
        }

        protected override void OnUnbind()
        {
            _timer?.Dispose();
            CoreContext.MasterServer.GameBroadcastReceived -= OnBroadcastReceived;
            base.OnUnbind();
        }
    }
}
