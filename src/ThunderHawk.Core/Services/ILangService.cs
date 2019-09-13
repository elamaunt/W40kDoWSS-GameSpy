using System;
using System.Globalization;

namespace ThunderHawk.Core
{
    public interface ILangService
    {
        event Action<CultureInfo> CultureChanged;
        CultureInfo CurrentCulture { get; set; }
        string GetString(string resourceName);
    }
}
