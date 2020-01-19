using System.Windows.Controls;
using System.Windows.Media;

namespace Framework.WPF
{
    internal class GridWithBackgroundBinder : BindingController<Border, IBackgroundFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IBackgroundFrame.BackgroundColor), OnBackgroundChanged);
            SubscribeOnPropertyChanged(Frame, nameof(IBackgroundFrame.BackgroundOpacity), OnOpacityChanged);
            OnBackgroundChanged();
        }

        void OnBackgroundChanged()
        {
            View.Background = (Brush) new BrushConverter().ConvertFromString(Frame.BackgroundColor);
            View.Background.Opacity = Frame.BackgroundOpacity;
        }
        
        void OnOpacityChanged()
        {
            View.Background.Opacity = Frame.BackgroundOpacity;
        }
    }
}
