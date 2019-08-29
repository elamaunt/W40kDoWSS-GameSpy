using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class LangService : ILangService
    {
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
