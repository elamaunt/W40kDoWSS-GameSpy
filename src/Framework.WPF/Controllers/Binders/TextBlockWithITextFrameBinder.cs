using System.Windows.Controls;

namespace Framework.WPF
{
    public class TextBlockWithITextFrameBinder : BindingController<TextBlock, ITextFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(ITextFrame.Text), OnTextChanged);
            OnTextChanged();
        }

        void OnTextChanged()
        {
            View.Text = Frame.Text;
        }
    }
}
