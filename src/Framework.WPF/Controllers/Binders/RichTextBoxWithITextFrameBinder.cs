using System.Windows.Controls;

namespace Framework.WPF
{
    public class RichTextBoxWithITextFrameBinder : BindingController<RichTextBox, ITextFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(ITextFrame.Text), OnTextChanged);
            OnTextChanged();
        }

        void OnTextChanged()
        {
            View.SelectAll();
            View.Cut();
            View.AppendText(Frame.Text);
        }
    }
}
