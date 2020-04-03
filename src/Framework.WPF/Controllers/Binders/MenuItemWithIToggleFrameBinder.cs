using System;
using System.Windows;
using System.Windows.Controls;

namespace Framework.WPF
{
    public class MenuItemWithIToggleFrameBinder : BindingController<MenuItem, IToggleFrame>
    {
        protected override void OnBind()
        {
            View.IsCheckable = true;
            View.IsChecked = Frame.IsChecked ?? false;

            View.Checked += OnViewChecked;
            View.Unchecked += OnViewUnchecked;
        }

        void OnViewUnchecked(object sender, RoutedEventArgs e)
        {
            Frame.IsChecked = false;
        }

        void OnViewChecked(object sender, RoutedEventArgs e)
        {
            Frame.IsChecked = true;
        }

        protected override void OnUnbind()
        {
            View.Checked -= OnViewChecked;
            View.IsCheckable = false;
            base.OnUnbind();
        }
    }
}
