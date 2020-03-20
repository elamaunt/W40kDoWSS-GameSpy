using Framework;
using Framework.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public partial class Window_Main : IGlobalNavigationManager, IMainWindowView
    {
        MainWindowViewModel ViewModel => base.ViewModel as MainWindowViewModel;

        private List<BindableWindow> createdWindows = new List<BindableWindow>();

        public Window_Main()
        {
            Bootstrapper.CurrentBatch.RegisterServiceFactory<IGlobalNavigationManager>(() => this);

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("thunderhaw_notify.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        public void OpenWindow(WindowViewModel viewModel)
        {
            var window = WPFPageHelper.InstantiateWindow(viewModel);
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }

        public void OpenPage<PageViewModelType>(Action<IDataBundle> inflateBundle = null)
            where PageViewModelType : PageViewModel, new()
        {
            var tabPageVM = ViewModel.Pages.DataSource.Select(x => x.ViewModel).OfType<PageViewModelType>().FirstOrDefault();
            var viewModel = tabPageVM ?? new PageViewModelType();

            PassDataIfNeeded(viewModel, inflateBundle);

            var panelFrame = NavigationFrame.GetBindedFrame<INavigationPanelFrame>();
            panelFrame.CurrentContentViewModel = viewModel;
        }

        public IBindableWindow OpenWindow<WindowViewModelType>(Action<IDataBundle> inflateBundle = null) 
            where WindowViewModelType : WindowViewModel, new()
        {
            var viewModel = new WindowViewModelType();
            PassDataIfNeeded(viewModel, inflateBundle);

            var window = WPFPageHelper.InstantiateWindow(viewModel);
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            createdWindows.Add(window);
            window.Show();
            return window;
        }

        public void CloseWindow(string windowName)
        {
            foreach (var window in createdWindows)
            {
                if (window.Title == windowName) 
                {
                    window.Close();
                }
            } 
        }

        public void GoBack()
        {
            if (NavigationFrame.CanGoBack)
                NavigationFrame.GoBack();
        }

        private void PassDataIfNeeded(PageViewModel viewModel, Action<IDataBundle> inflateBundle)
        {
            if (inflateBundle == null)
                return;

            var bundle = PageHelper.CreateBundle();
            inflateBundle(bundle);
            viewModel.PassData(bundle);
        }

    }
}
