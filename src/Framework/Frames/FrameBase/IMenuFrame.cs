using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    public interface IMenuFrame : IControlFrame
    {
        IEnumerable<IMenuItemFrame> MenuItems { get; }

        int ItemsCount { get; }
    }
}
