using System;
using System.Windows;

namespace SteamSpy.StaticClasses
{
    public static class LangService
    {
        public static string GetString(string resourceName)
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
