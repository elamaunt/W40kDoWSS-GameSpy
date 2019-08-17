using System;
using System.Windows;

namespace Framework.WPF
{
    public class UIElementWithIControlFrameBinder : BindingController<UIElement, IControlFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(Frame.Enabled), OnEnabledChanged);
            OnEnabledChanged();

            SubscribeOnPropertyChanged(Frame, nameof(Frame.Visible), OnVisibleChanged);
            OnVisibleChanged();
        }

        void OnEnabledChanged()
        {
            View.IsEnabled = Frame.Enabled;
        }

        void OnVisibleChanged()
        {
            View.Visibility = Frame.Visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
