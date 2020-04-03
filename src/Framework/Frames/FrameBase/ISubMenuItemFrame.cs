using System.Collections.Generic;

namespace Framework
{
    public interface ISubMenuItemFrame : IMenuItemFrame
    {
         IEnumerable<IMenuItemFrame> InnerItems { get; }
         int ItemsCount { get; }
    }
}
