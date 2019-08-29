using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Framework.WPF
{
    public class ViewModelTemplateSelector : DataTemplateSelector
    {
        public static readonly DataTemplateSelector Instance = new ViewModelTemplateSelector();

        readonly Dictionary<string, DataTemplate> _templates = new Dictionary<string, DataTemplate>();

        private ViewModelTemplateSelector()
        {
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var model = (ViewModel)item;

            var prefix = model.GetPrefix();
            var style = model.GetViewStyle();

            var key = prefix + style;

            if (_templates.TryGetValue(key, out DataTemplate template))
                return template;

            return _templates[key] = CreateTemplate(prefix, style);
        }

        DataTemplate CreateTemplate(string prefix, string style)
        {
            if (!WPFPageHelper.TryGetXamlPath(prefix, style, out string path))
                throw new Exception($"There is no xaml with name {prefix}_{style}.xaml");

            var xml = $@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:local=""clr-namespace:Framework.WPF;assembly=Framework.WPF""> 
                     <local:ElementContainer XamlPrefix=""{prefix}"" XamlStyle=""{style}"" />
                  </DataTemplate>";

            return (DataTemplate)XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
        }
    }
}
