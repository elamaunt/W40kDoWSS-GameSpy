using System;

namespace Framework
{
    public interface IGlobalNavigationManager
    {
        void ShowWindow<WindowViewModelType>(Action<IDataBundle> inflateBundle = null)
            where WindowViewModelType : WindowViewModel, new();
    }
}
