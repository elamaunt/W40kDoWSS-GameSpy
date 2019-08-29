using System;

namespace Framework
{
    public class ValueFrame<ValueType> : ControlFrame, IValueFrame<ValueType>
    {
        public Func<ValueType, string> ValueToTextConverter { get; set; }
        public Func<string, ValueType> TextToValueConverter { get; set; }

        private ValueType _value;
        public ValueType Value
        {
            get => _value;
            set
            {
                if (Equals(value, _value))
                    return;

                UpdateText(value);
                _value = value;
                FirePropertyChanged(nameof(Value));
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

                UpdateValue(value);
                _text = value;
                FirePropertyChanged(nameof(Text));
            }
        }

        private void UpdateText(ValueType value)
        {
            if (Equals(value, null))
            {
                _text = null;
                FirePropertyChanged(nameof(Text));
                return;
            }

            var converter = ValueToTextConverter ?? DefaultValueToTextConverter;
            _text = converter(value);
            FirePropertyChanged(nameof(Text));
        }

        private ValueType DefaultTextToValueConverter(string text)
        {
           return (ValueType)Convert.ChangeType(text, typeof(ValueType));
        }

        private void UpdateValue(string text)
        {
            if (text == null)
            {
                _value = default(ValueType);
                FirePropertyChanged(nameof(Text));
                return;
            }

            var converter = TextToValueConverter ?? DefaultTextToValueConverter;
            _value = converter(text);
            FirePropertyChanged(nameof(Value));
        }

        private string DefaultValueToTextConverter(ValueType value)
        {
            return Convert.ToString(value);
        }
    }
}
