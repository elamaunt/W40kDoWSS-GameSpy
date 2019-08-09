using Framework;
using ThunderHawk.Core;

namespace SteamSpy
{
    public class ApplicationModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterServiceFactory<ILangService>(() => new LangService());
        }
    }
}