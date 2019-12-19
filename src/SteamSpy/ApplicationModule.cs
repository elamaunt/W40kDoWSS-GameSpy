using Framework;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;

namespace ThunderHawk
{
    public class ApplicationModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterServiceFactory<IMasterServer>(() => new SingleMasterServer());
            batch.RegisterServiceFactory<IClientServer>(() => new SingleClientServer());
            
            batch.RegisterServiceFactory<ILangService>(() => new LangService());
            batch.RegisterServiceFactory<ILaunchService>(() => new LaunchService());
            batch.RegisterServiceFactory<ISteamApiService>(() => new SteamApiService());
            batch.RegisterServiceFactory<IUpdaterService>(() => new UpdaterService());

            batch.RegisterServiceFactory<ITweaksService>(() => new TweaksService());
            batch.RegisterServiceFactory<ISystemService>(() => new SystemService());
            batch.RegisterServiceFactory<IThunderHawkModManager>(() => new ThunderHawkModManager());
            batch.RegisterServiceFactory<IKeyValueStorage>(() => new ConfigKeyValueStorage());

            batch.RegisterServiceFactory<IResourcesService>(() => new ResourcesService());
            
            batch.RegisterControllerFactory(() => new TabControlWithListFrameBinder());
            batch.RegisterControllerFactory(() => new TabItemWithPageViewModelBinder());

            batch.RegisterControllerFactory(() => new MainWindowBackgroundController());
            batch.RegisterControllerFactory(() => new MainNewsPresentingController());
            batch.RegisterControllerFactory(() => new ChatUserColorController());
            batch.RegisterControllerFactory(() => new GamePlayersColorController());
            
            batch.RegisterControllerFactory(() => new PageExtendingController<IUserInteractions, UserInteractionsExtension>());
        }
    }
}