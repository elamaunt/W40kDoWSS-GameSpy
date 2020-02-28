using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class LangService : ILangService
    {
        public event Action<CultureInfo> CultureChanged;

        public LangService()
        {
            Reload();
        }

        private void Reload()
        {
            var langResource = Application.Current.Resources.MergedDictionaries.FirstOrDefault(x =>
                x.Source != null && x.Source.OriginalString.StartsWith("Resources"));
            if (langResource == null)
                return;

            langResource.Source = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru"
                ? new Uri("pack://application:,,,/ThunderHawk;component/Resources/Russian.xaml",
                    UriKind.Absolute)
                : new Uri("pack://application:,,,/ThunderHawk;component/Resources/English.xaml",
                    UriKind.Absolute);
        }

        public CultureInfo CurrentCulture
        {
            get => CultureInfo.CurrentCulture;
            set
            {
                CultureInfo.CurrentCulture = value;
                Reload();
                CultureChanged?.Invoke(value);
            }
        }

        public string GetString(string resourceName)
        {
            try
            {
                return Application.Current.FindResource(resourceName).ToString();
            }
            catch
            {
                return resourceName;
            }
        }
    }
}
