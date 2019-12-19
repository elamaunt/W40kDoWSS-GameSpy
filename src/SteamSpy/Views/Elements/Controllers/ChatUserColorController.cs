using Framework;
using System.Windows.Media;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ChatUserColorController : BindingController<Element_ChatUser, ChatUserItemViewModel>
    {
        static SolidColorBrush _connectedBrush = new SolidColorBrush(Color.FromRgb(40, 180, 40));
        static SolidColorBrush _connectingBrush = new SolidColorBrush(Color.FromRgb(180, 180, 40));
        static SolidColorBrush _disconnectedBrush = new SolidColorBrush(Color.FromRgb(180, 40, 40));
        static SolidColorBrush _unknownBrush = new SolidColorBrush(Color.FromRgb(40, 40, 40));

        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame.ActiveProfile, nameof(Frame.ActiveProfile.Value), OnActiveProfileChanged);
            SubscribeOnPropertyChanged(Frame.State, nameof(Frame.State.Value), OnStateChanged);
            OnActiveProfileChanged();
            OnStateChanged();
        }

        void OnStateChanged()
        {
            if (Frame.Info.IsUser)
            {
                View.ConnectIndicator.Fill = _connectedBrush;
                return;
            }

            switch (Frame.State.Value)
            {
                case UserState.Disconnected:
                    View.ConnectIndicator.Fill = _disconnectedBrush;
                    break;
                case UserState.Connecting:
                    View.ConnectIndicator.Fill = _connectingBrush;
                    break;
                case UserState.Connected:
                    View.ConnectIndicator.Fill = _connectedBrush;
                    break;
                default:
                    View.ConnectIndicator.Fill = _unknownBrush;
                    break;
            }
        }

        void OnActiveProfileChanged()
        {
            if (Frame.ActiveProfile.Value)
                View.Name.Foreground = new SolidColorBrush(Color.FromRgb(117,244,255));
            else
                View.Name.Foreground = new SolidColorBrush(Color.FromRgb(50, 53, 58));
        }
    }
}
