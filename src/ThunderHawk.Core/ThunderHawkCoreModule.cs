using Framework;

namespace ThunderHawk.Core
{
    public class ThunderHawkCoreModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            var mainPageController = new MainPageController();
            
            batch.RegisterControllerFactory(() => mainPageController);
            batch.RegisterControllerFactory(() => new MainWindowNavigationController());
            batch.RegisterControllerFactory(() => new MainWindowController());
            batch.RegisterControllerFactory(() => new SettingsWindowController());
            batch.RegisterControllerFactory(() => new AuthorizationWindowController(mainPageController));
            batch.RegisterControllerFactory(() => new RegistrationWindowController(mainPageController));
            batch.RegisterControllerFactory(() => new TweakItemController());
            batch.RegisterControllerFactory(() => new TweaksPageController());
            batch.RegisterControllerFactory(() => new FaqPageController());
            batch.RegisterControllerFactory(() => new ChatPageController());
            batch.RegisterControllerFactory(() => new ChatUserController());
            batch.RegisterControllerFactory(() => new StatsPageController());
            batch.RegisterControllerFactory(() => new LobbiesController());
            batch.RegisterControllerFactory(() => new GameItemController());
            
            batch.RegisterServiceFactory<IHttpService>(() => new ThunderHawkHttpService());
            batch.RegisterServiceFactory<IOpenLogsService>(() => new OpenLogsService());
        }
        
    }
}
