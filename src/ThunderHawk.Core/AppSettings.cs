using Framework;

namespace ThunderHawk.Core
{
    public static class AppSettings
    {
        static IKeyValueStorage Storage { get; } = Service<IKeyValueStorage>.Get();

        public static bool DisableFog
        {
            get => Storage.GetValue(nameof(DisableFog)).ConvertToOrDefault<bool>();
            set => Storage.SetValue(nameof(DisableFog), value.ToString());
        }

        public static bool ThunderHawkModAutoSwitch
        {
            get => Storage.GetValue(nameof(ThunderHawkModAutoSwitch)).ConvertToOrDefault<bool>();
            set => Storage.SetValue(nameof(ThunderHawkModAutoSwitch), value.ToString());
        }
    }
}
