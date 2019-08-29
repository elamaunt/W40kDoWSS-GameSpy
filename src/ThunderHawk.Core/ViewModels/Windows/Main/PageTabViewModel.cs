using Framework;

namespace ThunderHawk.Core
{
    public class PageTabViewModel : PageViewModel
    {
        public override ITextFrame Title => ViewModel.Title;

        public EmbeddedPageViewModel ViewModel { get; }

        public PageTabViewModel(EmbeddedPageViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public override string GetName()
        {
            return ViewModel.GetName();
        }

        public override string GetPrefix()
        {
            return ViewModel.GetPrefix();
        }

        public override string GetViewStyle()
        {
            return ViewModel.GetViewStyle();
        }
    }
}