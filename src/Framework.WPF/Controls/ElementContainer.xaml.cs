using System.Windows;
using System.Windows.Controls;

namespace Framework.WPF
{
    public partial class ElementContainer : UserControl
    {
        public static readonly DependencyProperty XamlPrefixProperty = DependencyProperty.Register("XamlPrefix", typeof(string), typeof(ElementContainer), new PropertyMetadata(null, OnValueChanged));

        public static readonly DependencyProperty XamlStyleProperty = DependencyProperty.Register("XamlStyle", typeof(string), typeof(ElementContainer), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var el = (ElementContainer)obj;
            el.CreateChildIfParametersReady();
        }

        object _child;

        public string XamlPrefix
        {
            get => GetValue(XamlPrefixProperty) as string;
        }

        
        public string XamlStyle
        {
            get => GetValue(XamlStyleProperty) as string;
        }

        public ElementContainer()
        {
            InitializeComponent();
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        void CreateChildIfParametersReady()
        {
            if (_child != null || XamlPrefix == null || XamlStyle == null)
                return;

            AddChild(_child = Service<IViewFactory>.Get().CreateView(XamlPrefix, XamlStyle));
        }
    }
}
