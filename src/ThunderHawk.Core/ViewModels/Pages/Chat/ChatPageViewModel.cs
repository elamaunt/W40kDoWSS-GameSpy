using Framework;

namespace ThunderHawk.Core
{
    public class ChatPageViewModel : EmbeddedPageViewModel
    {
        public ChatPageViewModel()
        {
            TitleButton.Text = CoreContext.LangService.GetString("ChatPage");

        }
    }
}
