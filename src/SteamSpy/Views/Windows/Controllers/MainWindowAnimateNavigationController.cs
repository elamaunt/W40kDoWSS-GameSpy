using Framework;
using Framework.WPF;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class MainWindowBackgroundController : BindingController<Window_Main, MainWindowViewModel>, ICustomContentPresenter
    {
        const string DefaultImagePath = "pack://application:,,,/ThunderHawk;component/Images/Background_Default.png";

        string _currentPath = DefaultImagePath;

        object _currentContent;

        protected override void OnBind()
        {
            Frame.NavigationPanel.SetExtension<ICustomContentPresenter>(this);
            SubscribeOnPropertyChanged(Frame.NavigationPanel, nameof(INavigationPanelFrame.CurrentContentViewModel), OnContentChanged);
            View.WindowBackground.Source = new BitmapImage(new Uri(DefaultImagePath, UriKind.RelativeOrAbsolute));

            View.NavigationFrame.Navigated += OnNavigated;
        }

        void OnNavigated(object sender, NavigationEventArgs e)
        {
            var uiElement = (UIElement)e.Content;

            if (_currentContent == uiElement)
                return;

            _currentContent = uiElement;

            if (uiElement != null)
            {
                uiElement.Opacity = 0;
                uiElement.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1d, TimeSpan.FromSeconds(0.25)));
            }
        }

        public void Present(System.Windows.Controls.Frame frame, IBindableView content)
        {
             var currentContent = (UIElement)frame.Content;

            if (currentContent == content)
                return;

             Action complete = () => frame.Content = content;

             if (currentContent != null)
             {
                 var fadeOutAnimation = new DoubleAnimation(0d, TimeSpan.FromSeconds(0.25));
                 fadeOutAnimation.Completed += (s, e) => complete();
                 currentContent.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
             }
             else
             {
                 complete();
             }
        }

        void OnContentChanged()
        {
            var content = Frame.NavigationPanel.CurrentContentViewModel;

            if (content == null)
            {
                SetupDefaultBackground();
                return;
            }

            var name = $"Background_{content.GetName()}";

            if (WPFPageHelper.TryGetImagePath(name, out string path))
                ChangeTo(path);
            else
                SetupDefaultBackground();
        }

        private void SetupDefaultBackground()
        {
            ChangeTo(DefaultImagePath);
        }

        void ChangeTo(string path)
        {
            if (_currentPath.Equals(path, StringComparison.Ordinal))
                return;

            _currentPath = path;

            if (!path.StartsWith("pack:"))
                path = "pack://application:,,,/ThunderHawk;component/" + path;
            ChangeSource(View.WindowBackground, new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)), TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.5));
        }

        public static void ChangeSource(Image image, ImageSource source, TimeSpan fadeOutTime, TimeSpan fadeInTime)
        {
            var fadeInAnimation = new DoubleAnimation(1d, fadeInTime);

            if (image.Source != null)
            {
                var fadeOutAnimation = new DoubleAnimation(0d, fadeOutTime);

                fadeOutAnimation.Completed += (o, e) =>
                {
                    image.Source = source;
                    image.BeginAnimation(Image.OpacityProperty, fadeInAnimation);
                };

                image.BeginAnimation(Image.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                image.Opacity = 0d;
                image.Source = source;
                image.BeginAnimation(Image.OpacityProperty, fadeInAnimation);
            }
        }

        protected override void OnUnbind()
        {
            View.NavigationFrame.Navigated -= OnNavigated;
            base.OnUnbind();
        }

        public void GoForward(System.Windows.Controls.Frame view)
        {
            var currentContent = (UIElement)view.Content;

            if (currentContent == null)
                return;

            Frame.GoForward.Enabled = false;
            var fadeOutAnimation = new DoubleAnimation(0d, TimeSpan.FromSeconds(0.25));

            fadeOutAnimation.Completed += (o, e) =>
            {
                view.GoForward();
                Frame.GoForward.Enabled = true;
            };

            currentContent.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
        }

        public void GoBack(System.Windows.Controls.Frame view)
        {
            var currentContent = (UIElement)view.Content;

            if (currentContent == null)
                return;

            Frame.GoBack.Enabled = false;
            var fadeOutAnimation = new DoubleAnimation(0d, TimeSpan.FromSeconds(0.25));

            fadeOutAnimation.Completed += (o, e) =>
            {
                view.GoBack();
                Frame.GoBack.Enabled = true;
            };

            currentContent.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
        }
    }
}
