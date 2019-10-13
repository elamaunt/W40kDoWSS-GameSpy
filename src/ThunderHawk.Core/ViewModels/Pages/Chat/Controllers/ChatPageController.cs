using System;
using System.Linq;
using System.Threading;
using Framework;

namespace ThunderHawk.Core
{
    public class ChatPageController : FrameController<ChatPageViewModel>
    {
        Timer _timer;
        volatile int _ticks;

        protected override void OnBind()
        {
            Frame.Send.Action = OnSendClicked;
            Frame.TextInput.Action = OnSendClicked;

            CoreContext.MasterServer.Connected += OnConnectionChanged;
            CoreContext.MasterServer.ConnectionLost += OnConnectionChanged;

            Frame.ConnectedLabel.Text = $"Соединение активно. Пользователей на сервере {Frame.Users.ItemsCount}";
            Frame.DisconnectedLabel.Text = "Соединение потеряно... идет попытка восстановления";
            OnConnectionChanged();

            _timer = new Timer(OnTime, null, 1000, 1000);
        }

        void OnTime(object state)
        {
            if (CoreContext.MasterServer.IsConnected)
                return;

            RunOnUIThread(() =>
            {
                _ticks++;

                Frame.DisconnectedLabel.Text = "Соединение потеряно... идет попытка восстановления" + string.Join(string.Empty, Enumerable.Repeat(".", _ticks));

                if (_ticks == 3)
                    _ticks = 0;
            });
        }

        void OnConnectionChanged()
        {
            RunOnUIThread(() =>
            {
                Frame.ConnectedLabel.Visible = CoreContext.MasterServer.IsConnected;
                Frame.DisconnectedLabel.Visible = !CoreContext.MasterServer.IsConnected;
            });
        }

        void OnSendClicked()
        {
            var text = Frame.TextInput.Text;

            if (text.IsNullOrWhiteSpace())
                return;

            CoreContext.MasterServer.SendChatMessage(text, false);

            Frame.TextInput.Text = string.Empty;
        }

        protected override void OnUnbind()
        {
            _timer?.Dispose();
            CoreContext.MasterServer.Connected -= OnConnectionChanged;
            CoreContext.MasterServer.ConnectionLost -= OnConnectionChanged;
            base.OnUnbind();
        }
    }
}
