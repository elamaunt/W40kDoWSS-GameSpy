namespace Framework
{
    public class ByteArrayFrame : ControlFrame
    {
        private byte[] _source;
        public byte[] Source
        {
            get => _source;
            set
            {
                if (value == _source)
                    return;

                _source = value;
                FirePropertyChanged(nameof(Source));
            }
        }
    }
}
