using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace Framework.WPF
{
    public class TextBoxBaseWithITextEditorFrameBinder : BindingController<TextBoxBase, ITextEditorFrame>
    {
        volatile bool _propertyChangedBlocked;
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(ITextFrame.Text), OnTextChanged);
            OnTextChanged();

            View.TextChanged += OnTextFromViewChanged;
            View.KeyUp += OnKeyUp;
        }

        void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                Frame.Action?.Invoke();
        }

        void OnTextFromViewChanged(object sender, TextChangedEventArgs e)
        {
            _propertyChangedBlocked = true;
            Frame.Text = GetTextValue(View);
            _propertyChangedBlocked = false;
        }

        string GetTextValue(TextBoxBase source)
        {
            // need to cast TextBoxBase to one of its implementations
            var txtControl = source as TextBox;

            if (txtControl == null)
            {
                var txtControlRich = source as RichTextBox;
                if (txtControlRich == null)
                    return null;

                return new TextRange(txtControlRich.Document.ContentStart, txtControlRich.Document.ContentEnd).Text;
            }

            return txtControl.Text;
        }

        void OnTextChanged()
        {
            if (_propertyChangedBlocked)
                return;

            View.SelectAll();
            View.Cut();
            View.AppendText(Frame.Text);
        }

        protected override void OnUnbind()
        {
            View.KeyUp -= OnKeyUp;
            View.TextChanged -= OnTextFromViewChanged;
            base.OnUnbind();
        }
    }
}
