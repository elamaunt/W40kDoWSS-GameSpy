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

        protected override void OnBind()
        {
            CoreContext.MasterServer.GameBroadcastReceived += OnBroadcastReceived;
            ReloadLobbies();
        }

        void OnBroadcastReceived(GameHostInfo host)
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
                .ContinueWith(task =>
                {
                    var source = new ObservableCollection<ItemViewModel>();

                    int maxPlayers = 0;

                    foreach (var host in task.Result.OrderBy(x => x.MaxPlayers).OrderBy(x => x.Players))
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
                })
                .OnCompletedOnUi(models => Frame.GamesInAuto.DataSource = models);
        }

        protected override void OnUnbind()
        {
            CoreContext.MasterServer.GameBroadcastReceived -= OnBroadcastReceived;
            base.OnUnbind();
        }
    }
}
