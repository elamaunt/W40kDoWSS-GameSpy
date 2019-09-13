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
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru")
                Application.Current.Resources.MergedDictionaries[6].Source = new Uri("pack://application:,,,/ThunderHawk;component/Resources/Russian.xaml", UriKind.Absolute);
            else
                Application.Current.Resources.MergedDictionaries[6].Source = new Uri("pack://application:,,,/ThunderHawk;component/Resources/English.xaml", UriKind.Absolute);
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
