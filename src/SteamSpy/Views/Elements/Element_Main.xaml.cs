using Framework;
using Framework.WPF;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public partial class Element_Main : IMainPage
    {
        public void OpenWindow(WindowViewModel viewModel)
        {
            WPFPageHelper.InstantiateWindow(viewModel).Show();
        }
    }
}
