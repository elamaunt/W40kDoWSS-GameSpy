using Framework;
using Framework.WPF;

namespace ThunderHawk
{
    public interface IControlExtension : IExtension
    {
        void OnExtended(BindableControl view);
    }
}
