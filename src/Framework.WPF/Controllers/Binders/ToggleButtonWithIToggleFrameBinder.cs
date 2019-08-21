using System.Windows;
using System.Windows.Controls.Primitives;

namespace Framework.WPF
{
    public class ToggleButtonWithIToggleFrameBinder : BindingController<ToggleButton, IToggleFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IToggleFrame.IsChecked), OnCheckedChanged);
            OnCheckedChanged();

            View.Unchecked += OnViewCheckedChanged;
            View.Checked += OnViewCheckedChanged;
        }

        void OnViewCheckedChanged(object sender, RoutedEventArgs e)
        {
            Frame.IsChecked = View.IsChecked;
        }

        void OnCheckedChanged()
        {
            View.IsChecked = Frame.IsChecked;
        }

        protected override void OnUnbind()
        {
            View.Checked -= OnViewCheckedChanged;
            base.OnUnbind();
        }
    }
}
