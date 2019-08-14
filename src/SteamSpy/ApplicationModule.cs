using Framework;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ApplicationModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterServiceFactory<ILangService>(() => new LangService());
            batch.RegisterServiceFactory<ILaunchService>(() => new LaunchService());
            batch.RegisterControllerFactory(() => new TabControlWithListFrameBinder());
            batch.RegisterControllerFactory(() => new TabItemWithPageViewModelBinder());
        }
    }
}