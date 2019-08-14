using System.Windows.Controls.Primitives;

namespace Framework.WPF
{
    internal class ButtonWithITextFrameBinder : BindingController<ButtonBase, ITextFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(ITextFrame.Text), OnTextChanged);
            OnTextChanged();
        }

        void OnTextChanged()
        {
            View.Content = Frame.Text;
        }
    }
}
