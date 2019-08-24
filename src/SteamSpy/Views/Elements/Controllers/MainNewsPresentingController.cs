using Framework;
using Framework.WPF;
using System;
using System.Windows;
using System.Windows.Media.Animation;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class MainNewsPresentingController : FrameController<MainPageViewModel>, ICustomItemPresenter
    {
        protected override void OnBind()
        {
            Frame.News.SetExtension<ICustomItemPresenter>(this);
        }

        public void Present(FrameworkElement parent, FrameworkElement cell)
        {
            cell.Opacity = 0;
            cell.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.4)));

            cell.Margin = new Thickness(0,20,0,0);
            var duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
            var anim = new ThicknessAnimation(new Thickness(0,0,0,0), duration);
            cell.BeginAnimation(FrameworkElement.MarginProperty, anim);
        }
    }
}
