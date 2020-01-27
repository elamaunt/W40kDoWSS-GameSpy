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
            batch.RegisterControllerFactory(() => new NewsItemController());
            batch.RegisterControllerFactory(() => new NewsViewerController());
            batch.RegisterControllerFactory(() => new ChatPageController());
            batch.RegisterControllerFactory(() => new ChatUserController());
            batch.RegisterControllerFactory(() => new StatsPageController());
            batch.RegisterControllerFactory(() => new LobbiesController());
            batch.RegisterControllerFactory(() => new GameItemController());
            batch.RegisterControllerFactory(() => new InGamePageController());
            
            batch.RegisterServiceFactory<IHttpService>(() => new ThunderHawkHttpService());
            batch.RegisterServiceFactory<INewsProvider>(() => new TestNewsProvider());
            batch.RegisterServiceFactory<IOpenLogsService>(() => new OpenLogsService());
        }
        
    }
}
