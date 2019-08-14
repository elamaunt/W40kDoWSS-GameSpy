using Framework;

namespace ThunderHawk.Core
{
    public class ThunderHawkCoreModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterControllerFactory(() => new MainPageController());
            batch.RegisterControllerFactory(() => new MainWindowTabsController());
            batch.RegisterControllerFactory(() => new MainWindowController());
            
            batch.RegisterServiceFactory<INewsProvider>(() => new TestNewsProvider());
            batch.RegisterServiceFactory<IOptionsService>(() => new OptionsProvider());
        }
    }
}
