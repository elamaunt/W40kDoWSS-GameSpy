using Framework;

namespace ThunderHawk.Core
{
    public class FAQPageViewModel : EmbeddedPageViewModel
    {
        public ListFrame<QuestionItemViewModel> Questions { get; } = new ListFrame<QuestionItemViewModel>();
    }
}
