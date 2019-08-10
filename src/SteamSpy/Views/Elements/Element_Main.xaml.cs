using Framework;
using Framework.WPF;
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
    }
}
