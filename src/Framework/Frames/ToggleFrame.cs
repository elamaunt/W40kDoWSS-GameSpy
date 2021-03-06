﻿namespace Framework
{
    public class ToggleFrame : ControlFrame, IToggleFrame
    {
        private bool? _isChecked;
        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked)
                    return;

                _isChecked = value;
                FirePropertyChanged(nameof(IsChecked));
            }
        }
    }
}
