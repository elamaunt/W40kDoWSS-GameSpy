using Framework;

namespace ThunderHawk.Core
{
    public class TweakItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public TextFrame Description { get; } = new TextFrame();
        public TextFrame ApplyText { get; } = new TextFrame();
        public TextFrame RestoreText { get; } = new TextFrame();

        public TweakItemViewModel(string name, string description, string applyText, string restoreText)
        {
            Name.Text = name;
            Description.Text = description;
            ApplyText.Text = applyText;
            RestoreText.Text = restoreText;
        }
    }
}
