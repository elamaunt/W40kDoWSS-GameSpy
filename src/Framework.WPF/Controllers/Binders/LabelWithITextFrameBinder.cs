using System.Windows.Controls;

namespace Framework.WPF
{
    internal class LabelWithITextFrameBinder : BindingController<Label, ITextFrame>
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
