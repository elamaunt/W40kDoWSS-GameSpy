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
        
        public static bool DesktopNotificationsEnabled
        {
            get => Storage.GetValue(nameof(DesktopNotificationsEnabled)).ConvertToOrDefault<bool>();
            set => Storage.SetValue(nameof(DesktopNotificationsEnabled), value.ToString());
        }

        public static bool ThunderHawkModAutoSwitch
        {
            get => Storage.GetValue(nameof(ThunderHawkModAutoSwitch)).ConvertToOrDefault<bool>();
            set => Storage.SetValue(nameof(ThunderHawkModAutoSwitch), value.ToString());
        }

        public static bool LaunchThunderHawkAtStartup
        {
            get => CoreContext.SystemService.CheckIsItInStartup();
            set
            {
                if (value)
                    CoreContext.SystemService.AddInStartup();
                else
                    CoreContext.SystemService.RemoveFromStartup();
            }
        }

        public static bool IsFirstLaunch
        {
            get => Storage.GetValue(nameof(IsFirstLaunch)).ConvertToOrDefault<bool>();
            set => Storage.SetValue(nameof(IsFirstLaunch), value.ToString());
        }
        
    }
}
