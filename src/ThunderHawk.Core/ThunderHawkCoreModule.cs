using Framework;

namespace ThunderHawk.Core
{
    public class ThunderHawkCoreModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterControllerFactory(() => new MainPageController());
            batch.RegisterControllerFactory(() => new MainWindowNavigationController());
            batch.RegisterControllerFactory(() => new MainWindowController());
            batch.RegisterControllerFactory(() => new SettingsWindowController());

            batch.RegisterServiceFactory<INewsProvider>(() => new TestNewsProvider());
            batch.RegisterServiceFactory<IOptionsService>(() => new OptionsService());
        }
    }
}
