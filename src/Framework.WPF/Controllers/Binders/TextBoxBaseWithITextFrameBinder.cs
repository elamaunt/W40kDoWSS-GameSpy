using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Framework.WPF
{
    public class TextBoxBaseWithITextFrameBinder : BindingController<TextBoxBase, ITextFrame>
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
