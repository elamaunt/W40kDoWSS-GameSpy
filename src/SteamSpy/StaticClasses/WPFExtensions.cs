using System;
using System.Windows;
using System.Windows.Interop;

namespace SteamSpy.StaticClasses
{
    internal static class WPFExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            if (((FrameworkElement)templateFrameworkElement).TemplatedParent is Window window) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }
}
