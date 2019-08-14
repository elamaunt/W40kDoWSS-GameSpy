using System.Windows.Controls.Primitives;

namespace Framework.WPF
{
    public class ToggleButtonWithIToggleFrameBinder : BindingController<ToggleButton, IToggleFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IToggleFrame.IsChecked), OnCheckedChanged);
            OnCheckedChanged();
        }

        void OnCheckedChanged()
        {
            View.IsChecked = Frame.IsChecked;

        }
    }
}
