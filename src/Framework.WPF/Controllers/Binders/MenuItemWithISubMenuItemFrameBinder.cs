using System.Windows.Controls;

namespace Framework.WPF
{
    public class MenuItemWithISubMenuItemFrameBinder : BindingController<MenuItem, ISubMenuItemFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(ISubMenuItemFrame.InnerItems), OnInnerItemsChanged);
            OnInnerItemsChanged();
        }

        void OnInnerItemsChanged()
        {
            ClearItems();
            var items = Frame.InnerItems;

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
