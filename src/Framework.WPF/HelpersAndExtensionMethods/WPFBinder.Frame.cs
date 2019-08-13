using System;
using System.Windows;

namespace Framework.WPF
{
    public static partial class WPFBinder
    {
        public static readonly DependencyProperty FrameProperty = DependencyProperty.RegisterAttached("Frame", typeof(object), typeof(FrameworkElement), new PropertyMetadata(null, OnBind));

        private static void OnBind(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var binding = FrameBinder.GetBindedFrame(target);

            if (binding == args.NewValue)
                return;

            if (binding != null)
                FrameBinder.Unbind(target);

            if (args.NewValue != null)
                FrameBinder.Bind(target, args.NewValue);
        }

        public static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var binding = FrameBinder.GetBindedFrame(sender);

            if (binding == args.NewValue)
                return;

            if (binding != null)
                FrameBinder.Unbind(sender);

            if (args.NewValue != null)
                FrameBinder.Bind(sender, args.NewValue);
        }

        public static object GetFrame(this FrameworkElement element)
        {
            return element.GetValue(FrameProperty);
        }

        public static void SetFrame(this FrameworkElement element, string name)
        {
            element.SetBinding(FrameProperty, name);
        }
    }
}
