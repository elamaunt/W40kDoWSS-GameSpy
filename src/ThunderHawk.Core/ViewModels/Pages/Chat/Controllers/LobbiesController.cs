using Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            CoreContext.MasterServer.LoadLobbies(null, CoreContext.ClientServer.GetIndicator())
                .ContinueWith(task =>
                {
                    var lobbies = task.Result;

                    var hosts = new GameHostInfo[lobbies.Length];

                    for (int i = 0; i < hosts.Length; i++)
                    {
                        var lobby = lobbies[i];

                        var host = new GameHostInfo();

                        host.IsUser = lobby.HostSteamId == CoreContext.SteamApi.GetUserSteamId();
                        host.MaxPlayers = lobby.MaxPlayers.ParseToIntOrDefault();
                        host.Players = lobby.PlayersCount.ParseToIntOrDefault();
                        host.Ranked = lobby.Ranked;
                        host.GameVariant = lobby.GameVariant;
                        host.Teamplay = lobby.IsTeamplay;

                        hosts[i] = host;
                    }

                    return hosts;
                }, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(t => ToSource(t.Result))
                .OnCompletedOnUi(models => Frame.GamesInAuto.DataSource = models);
        }

        private ObservableCollection<ItemViewModel> ToSource(GameHostInfo[] hosts)
        {
            var source = new ObservableCollection<ItemViewModel>();

            int maxPlayers = 0;
            String gameVariant = "";

            foreach (var host in hosts.OrderBy(x => x.MaxPlayers).ThenBy(x => x.GameVariant).ThenBy(x => x.Players))
            {
                if (!host.Ranked)
                    continue;

                // эта логика для отображения заголовков 1х1 - thunderhawk, 2x2 - vanila и т.д.
                if (maxPlayers != host.MaxPlayers || gameVariant != host.GameVariant)
                {
                    maxPlayers = host.MaxPlayers;
                    gameVariant = host.GameVariant;

                    var separator = new GamesSeparatorItemViewModel();
                    separator.Header.Text = $"{maxPlayers / 2}vs{maxPlayers / 2} - " + getHumanReadableGameMode(gameVariant);
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
        
        private String getHumanReadableGameMode(String gameVariant)
        {
            if (gameVariant.Contains("thunderhawk"))
            {
                return "Thunderhawk - " + gameVariant.Substring(0, 5);
            }
            if (gameVariant.Contains("jbugfixmod"))
            {
                return "Classic bug fix";
            }
            return gameVariant;
        }
    }
}
