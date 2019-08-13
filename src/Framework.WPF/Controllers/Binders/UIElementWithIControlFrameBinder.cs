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
        }

        void OnEnabledChanged()
        {
            View.IsEnabled = Frame.Enabled;
        }
    }
}
