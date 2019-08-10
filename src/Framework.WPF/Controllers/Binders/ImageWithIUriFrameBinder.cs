using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Framework.WPF
{
    public class ImageWithIUriFrameBinder : BindingController<Image, IUriFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(Frame.Uri), OnUriChanged);
            OnUriChanged();
        }
        void OnUriChanged()
        {
            View.Source = new BitmapImage(Frame.Uri);
        }
    }
}
