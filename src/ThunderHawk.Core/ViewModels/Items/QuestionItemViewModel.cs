using Framework;

namespace ThunderHawk.Core
{
    public class QuestionItemViewModel : ItemViewModel
    {
        public ToggleButtonFrame Question { get; } = new ToggleButtonFrame();
        public TextFrame Answer { get; } = new TextFrame();

        public QuestionItemViewModel(string question, string answer)
        {
            Question.Text = "+ " + question;
            Answer.Text =answer;
            Question.IsChecked = false;
            Answer.Visible = Question.IsChecked ?? false;

            if (Answer.Visible)
                Question.Text = "- " + question;
            else
                Question.Text = "+ " + question;

            Question.Action = () =>
            {
                if (Question.IsChecked ?? false)
                {
                    Answer.Visible = true;
                    Question.Text = "- " + question;
                }
                else
                {
                    Answer.Visible = false;
                    Question.Text = "+ " + question;
                }
            };
        }
    }
}
