namespace Framework
{
    public class TextFrame : ControlFrame, ITextFrame
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;

                _text = value;
                FirePropertyChanged(nameof(Text));
            }
        }
    }
}
