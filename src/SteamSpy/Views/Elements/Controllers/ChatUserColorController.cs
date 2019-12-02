using System;
using System.Windows.Media;
using Framework;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ChatUserColorController : BindingController<Element_ChatUser, ChatUserItemViewModel>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame.ActiveProfile, nameof(Frame.ActiveProfile.Value), OnActiveProfileChanged);
            OnActiveProfileChanged();
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
