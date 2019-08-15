using System;

namespace Framework
{
    public interface IGlobalNavigationManager
    {
        void OpenPage<PageViewModelType>(Action<IDataBundle> inflateBundle = null)
            where PageViewModelType : PageViewModel, new();

        void GoBack();

        IBindableWindow OpenWindow<WindowViewModelType>(Action<IDataBundle> inflateBundle = null)
            where WindowViewModelType : WindowViewModel, new();
    }
}
