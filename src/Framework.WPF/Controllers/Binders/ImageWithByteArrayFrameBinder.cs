using System.Windows.Controls;

namespace Framework.WPF
{
    internal class ImageWithByteArrayFrameBinder : BindingController<Image, ByteArrayFrame>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(ByteArrayFrame.Source), OnSourceChanged);
            OnSourceChanged();
        }
        
        private void OnSourceChanged()
        {
            var source = Frame.Source;

            if (source == null)
            {
                View.Source = null;
                return;
            }

            View.Source = ImageHelper.CreateImageFromSource(source);
        }
    }
}
