using System.Windows;

namespace Framework.WPF
{
    public static partial class WPFBinder
    {
        public static readonly DependencyProperty ItemPrefixProperty = DependencyProperty.RegisterAttached("ItemPrefix", typeof(string), typeof(FrameworkElement), new PropertyMetadata(null));
        
        public static string GetItemPrefix(this FrameworkElement element)
        {
            return (string)element.GetValue(ItemPrefixProperty);
        }

        public static void SetItemPrefix(this FrameworkElement element, string prefix)
        {
            element.SetValue(ItemPrefixProperty, prefix);
        }
    }
}
