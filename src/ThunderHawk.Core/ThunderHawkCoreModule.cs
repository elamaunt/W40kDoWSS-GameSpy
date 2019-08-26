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
            batch.RegisterControllerFactory(() => new TweakItemController());
            batch.RegisterControllerFactory(() => new TweaksPageController());
            batch.RegisterControllerFactory(() => new FaqPageController());

            batch.RegisterServiceFactory<IHttpService>(() => new ThunderHawkHttpService());
            batch.RegisterServiceFactory<INewsProvider>(() => new TestNewsProvider());
        }
    }
}
