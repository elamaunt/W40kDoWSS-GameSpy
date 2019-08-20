using Framework;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class ApplicationModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterServiceFactory<ILangService>(() => new LangService());
            batch.RegisterServiceFactory<ILaunchService>(() => new LaunchService());

            batch.RegisterServiceFactory<ITweaksService>(() => new TweaksService());
            batch.RegisterServiceFactory<ISystemService>(() => new SystemService());

            batch.RegisterControllerFactory(() => new TabControlWithListFrameBinder());
            batch.RegisterControllerFactory(() => new TabItemWithPageViewModelBinder());

            batch.RegisterControllerFactory(() => new MainWindowBackgroundController());
            batch.RegisterControllerFactory(() => new MainNewsPresentingController());
        }
    }
}