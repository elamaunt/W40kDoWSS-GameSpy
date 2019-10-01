using Framework;

namespace ThunderHawk.Core
{
    public class ChatPageViewModel : EmbeddedPageViewModel
    {
        public TextFrame ConnectedLabel { get; } = new TextFrame();
        public TextFrame DisconnectedLabel { get; } = new TextFrame();
        public TextFrame ActiveNickName { get; } = new TextFrame();
        public TextFrame Score1v1 { get; } = new TextFrame();
        public TextFrame GamesCount { get; } = new TextFrame();
        public TextFrame WinCount { get; } = new TextFrame();
        public TextFrame Winrate { get; } = new TextFrame();
        public ListFrame<ChatUserItemViewModel> Users { get; } = new ListFrame<ChatUserItemViewModel>();
        public ListFrame<ChatMessageItemViewModel> Messages { get; } = new ListFrame<ChatMessageItemViewModel>();

        public TextEditorFrame TextInput { get; } = new TextEditorFrame();
        public ButtonFrame Send { get; } = new ButtonFrame() { Text = "Отправить" };

        public ChatPageViewModel()
        {
            TitleButton.Text = CoreContext.LangService.GetString("ChatPage");
        }

        public IScrollManager MessagesScrollManager => Messages.GetExtension<IScrollManager>();
        public IScrollManager UsersScrollManager => Users.GetExtension<IScrollManager>();
    }
}
