namespace Framework
{
    public class ControlFrame : Frame, IControlFrame
    {
        bool _visible = true;
        bool _enabled = true;

        public bool Visible
        {
            get => _visible;
            set
            {
                if (value == _visible)
                    return;

                _visible = value;
                FirePropertyChanged(nameof(Visible));
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled)
                    return;

                _enabled = value;
                FirePropertyChanged(nameof(Enabled));
            }
        }

        IMenuFrame _menu;
        public IMenuFrame ContextMenu
        {
            get => _menu;
            set
            {
                if (_menu == value)
                    return;
                _menu = value;
                FirePropertyChanged(nameof(ContextMenu));
            }
        }
    }
}
