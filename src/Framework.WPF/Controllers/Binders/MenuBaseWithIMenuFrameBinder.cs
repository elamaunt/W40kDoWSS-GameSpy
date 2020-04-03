using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Framework.WPF
{
    public class MenuBaseWithIMenuFrameBinder : BindingController<MenuBase, IMenuFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IMenuFrame.MenuItems), OnMenuItemsChanged);
            OnMenuItemsChanged();
        }

        void OnMenuItemsChanged()
        {
            ClearItems();
            var items = Frame.MenuItems;

            if (items.IsNullOrEmpty())
                return;

            foreach (var item in items)
            {
                var newMenuItem = new MenuItem();
                FrameBinder.Bind(newMenuItem, item);
                View.Items.Add(newMenuItem);
            }
        }

        void ClearItems()
        {
            foreach (var item in View.Items)
                FrameBinder.Unbind(item);

            View.Items.Clear();
        }

        protected override void OnUnbind()
        {
            ClearItems();
            base.OnUnbind();
        }
    }
}
