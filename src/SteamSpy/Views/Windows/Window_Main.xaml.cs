﻿using Framework;
using Framework.WPF;
using System;
using System.Linq;
using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public partial class Window_Main : IGlobalNavigationManager
    {
        MainWindowViewModel ViewModel => base.ViewModel as MainWindowViewModel;

        public Window_Main()
        {
            Bootstrapper.CurrentBatch.RegisterServiceFactory<IGlobalNavigationManager>(() => this);
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
            window.Show();
            return window;
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
