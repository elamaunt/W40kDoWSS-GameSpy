using System.Windows;

namespace Framework.WPF
{
    public static partial class WPFBinder
    {
        public static readonly DependencyProperty ItemStyleProperty = DependencyProperty.RegisterAttached("ItemStyle", typeof(string), typeof(FrameworkElement), new PropertyMetadata(null));
        
        public static string GetItemStyle(this FrameworkElement element)
        {
            return (string)element.GetValue(ItemStyleProperty);
        }

        public static void SetItemStyle(this FrameworkElement element, string style)
        {
            element.SetValue(ItemStyleProperty, style);
        }
    }
}
