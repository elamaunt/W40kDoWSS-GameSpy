using Framework;

namespace ThunderHawk.Core
{
    public class QuestionItemViewModel : ItemViewModel
    {
        public ToggleButtonFrame Question { get; } = new ToggleButtonFrame();
        public TextFrame Answer { get; } = new TextFrame();

        public QuestionItemViewModel(string question, string answer)
        {
            Question.Text = question;
            Answer.Text = answer;
            Question.IsChecked = false;
            Answer.Visible = Question.IsChecked ?? false;
            Question.Action = () => Answer.Visible = Question.IsChecked ?? false;
        }
    }
}
