using System;

namespace Framework
{
    public class BackgroundFrame : ControlFrame, IBackgroundFrame
    {
        private string _backgroundColor = "#bdbebd";
        private double _backgroundOpacity = 1;

        public string BackgroundColor
        {
            get => _backgroundColor;
            set
            {

                _backgroundColor = value;
                FirePropertyChanged(nameof(BackgroundColor));
            }
        }

        public double BackgroundOpacity {
            get => _backgroundOpacity;
            set
            {

                _backgroundOpacity = value;
                FirePropertyChanged(nameof(BackgroundOpacity));
            }
        }

        
    }
}
