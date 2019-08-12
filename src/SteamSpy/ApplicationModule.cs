﻿using Framework;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ApplicationModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterServiceFactory<ILangService>(() => new LangService());
            batch.RegisterControllerFactory(() => new TabControlWithListFrameBinder());
            batch.RegisterControllerFactory(() => new TabItemWithPageViewModelBinder());
        }
    }
}