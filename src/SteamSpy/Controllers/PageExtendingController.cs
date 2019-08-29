using Framework;
using Framework.WPF;

namespace ThunderHawk
{
    public class PageExtendingController<ExtensionInterfaceType, ExtensionImplementationType> : ExtendController<BindableControl, ExtensionInterfaceType, ExtensionImplementationType>
        where ExtensionInterfaceType : class
        where ExtensionImplementationType : ExtensionInterfaceType, IControlExtension, new()
    {
        protected override void OnExtended()
        {
            Extension.OnExtended(View);
        }
    }
}
