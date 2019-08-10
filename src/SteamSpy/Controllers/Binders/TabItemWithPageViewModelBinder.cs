using Framework;
using System;
using System.Windows.Controls;

namespace ThunderHawk
{
    public class TabItemWithPageViewModelBinder : BindingController<TabItem, PageViewModel>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame.Title, nameof(Frame.Title.Text), OnTitleTextChanged);
            OnTitleTextChanged();
        }

        void OnTitleTextChanged()
        {
            View.Header = Frame.Title.Text;
        }
    }
}
