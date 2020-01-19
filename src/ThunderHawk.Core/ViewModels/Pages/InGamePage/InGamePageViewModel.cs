using System;
using System.ComponentModel;
using Framework;
using ThunderHawk.Core.Frames;

namespace ThunderHawk.Core
{ 
    public class InGamePageViewModel : EmbeddedPageViewModel
    {
        
        public ButtonFrame Change { get; } = new ButtonFrame() { Text = "Refresh" };
        public TextFrame InGameLabel { get; } = new TextFrame();
        
        public TextFrame ApmLabel { get; } = new TextFrame();
        
        public TextFrame InfoLabel { get; } = new TextFrame();
        public PlayerFrameInGame Player0 { get; } = new PlayerFrameInGame();
        public PlayerFrameInGame Player1 { get; } = new PlayerFrameInGame();
        public PlayerFrameInGame Player2 { get; } = new PlayerFrameInGame();
        public PlayerFrameInGame Player3 { get; } = new PlayerFrameInGame();
        
        public PlayerFrameInGame Player4 { get; } = new PlayerFrameInGame();
        public PlayerFrameInGame Player5 { get; } = new PlayerFrameInGame();
        
        public PlayerFrameInGame Player6 { get; } = new PlayerFrameInGame();
        public PlayerFrameInGame Player7 { get; } = new PlayerFrameInGame();
        
        public UriFrame Map { get; } = new UriFrame();

        public InGamePageViewModel()
        {         
            TitleButton.Text = CoreContext.LangService.GetString("InGamePage");
            Map.Uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Maps/default.jpg");
            
        }
    }
}
