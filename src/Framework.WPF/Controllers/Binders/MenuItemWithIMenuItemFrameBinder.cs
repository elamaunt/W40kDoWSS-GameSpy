using System.Windows.Controls;

namespace Framework.WPF
{
    public class MenuItemWithIMenuItemFrameBinder : BindingController<MenuItem, IMenuItemFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IMenuItemFrame.Text), OnTextChanged);
            OnTextChanged();
        }

        void OnTextChanged()
        {
            View.Header = Frame.Text;
        }
    }
}
