using Framework;
using System.Windows.Controls;

namespace ThunderHawk
{
    public class TabItemWithPageViewModelBinder : BindingController<TabItem, PageViewModel>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame.Title, nameof(Frame.Title.Text), OnTitleTextChanged);
            SubscribeOnPropertyChanged(Frame, nameof(Frame.Enabled), OnEnabledChanged);
            OnTitleTextChanged();
            OnEnabledChanged();
        }

        void OnEnabledChanged()
        {
            View.IsEnabled = Frame.Enabled;
        }

        void OnTitleTextChanged()
        {
            View.Header = Frame.Title.Text;
        }
    }
}
