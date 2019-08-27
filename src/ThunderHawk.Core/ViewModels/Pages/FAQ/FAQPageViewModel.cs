using Framework;

namespace ThunderHawk.Core
{
    public class FAQPageViewModel : EmbeddedPageViewModel
    {
        public TextFrame Quote { get; } = new TextFrame() { Text = "<i>Задавать вопросы — значит сомневаться.</i>" };

        public ListFrame<QuestionItemViewModel> Questions { get; } = new ListFrame<QuestionItemViewModel>();

        public FAQPageViewModel()
        {
            TitleButton.Text = "FAQ";
        }
    }
}
