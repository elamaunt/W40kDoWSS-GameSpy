using System.Windows;
using System.Windows.Controls;

namespace Framework.WPF
{
    public class FrameworkElementWithIControlFrameBinder : BindingController<FrameworkElement, IControlFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(Frame.Enabled), OnEnabledChanged);
            OnEnabledChanged();

            SubscribeOnPropertyChanged(Frame, nameof(Frame.Visible), OnVisibleChanged);
            OnVisibleChanged();

            SubscribeOnPropertyChanged(Frame, nameof(Frame.ContextMenu), OnContextMenuChanged);
            OnContextMenuChanged();
        }

        void OnContextMenuChanged()
        {
            var menu = Frame.ContextMenu;
            var currentMenu = View.ContextMenu;

            if (currentMenu != null)
                FrameBinder.Unbind(currentMenu);

            if (menu == null)
            {
                View.ContextMenu = null;
            }
            else
            {
                var newMenu = new ContextMenu();
                FrameBinder.Bind(newMenu, menu);
                View.ContextMenu = newMenu;
            }
        }

        void OnEnabledChanged()
        {
            View.IsEnabled = Frame.Enabled;
        }

        void OnVisibleChanged()
        {
            View.Visibility = Frame.Visible ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnUnbind()
        {
            var currentMenu = View.ContextMenu;

            if (currentMenu != null)
                FrameBinder.Unbind(currentMenu);

            base.OnUnbind();
        }
    }
}
