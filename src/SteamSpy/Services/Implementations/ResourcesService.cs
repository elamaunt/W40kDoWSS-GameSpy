using Framework.WPF;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ResourcesService : IResourcesService
    {
        public bool HasImageWithName(string name)
        {
            return WPFPageHelper.IsImageExists(name);
        }
    }
}
