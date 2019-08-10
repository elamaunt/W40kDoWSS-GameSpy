using System;

namespace Framework
{
    public class UriFrame : ControlFrame, IUriFrame
    {
        private Uri _uri;
        public Uri Uri
        {
            get => _uri;
            set
            {
                if (Equals(value, _uri))
                    return;

                if (value == null)
                    _text = null;
                else
                    _text = value.OriginalString;

                _uri = value;
                FirePropertyChanged(nameof(Uri));
                FirePropertyChanged(nameof(Text));
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;

                if (value == null)
                    _uri = null;
                else
                    _uri = new Uri(value, UriKind.RelativeOrAbsolute);
                _text = value;
                FirePropertyChanged(nameof(Text));
                FirePropertyChanged(nameof(Text));
            }
        }
    }
}
