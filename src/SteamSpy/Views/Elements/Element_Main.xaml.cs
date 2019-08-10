using Framework;
using Framework.WPF;
using System;
using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public partial class Element_Main : IMainPage
    {
        public void OpenWindow(WindowViewModel viewModel)
        {
            var window = WPFPageHelper.InstantiateWindow(viewModel);
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            /*System.Threading.Tasks.Task.Delay(200).ContinueWith(t =>
            {
                Dispatcher.Invoke(() => VisualParent.InvalidateVisual());
            });*/
        }
    }
}
