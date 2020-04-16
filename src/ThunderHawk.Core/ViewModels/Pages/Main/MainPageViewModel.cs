using System;
using System.ComponentModel;
using Framework;
using ThunderHawk.Core.Frames;

namespace ThunderHawk.Core
{
    public class MainPageViewModel : EmbeddedPageViewModel, INotifyPropertyChanged
    {
        public ControlFrame LoadingIndicator { get; } = new ControlFrame() { Visible = false };
        public ListFrame<NewsItemViewModel> News { get; } = new ListFrame<NewsItemViewModel>();
        public ButtonFrame LaunchGame { get; } = new ButtonFrame() { Text = "Launch Thunderhawk" };
        public ActionFrame FAQLabel { get; } = new ActionFrame();

        public ActionFrame Tweaks { get; } = new ActionFrame();

        public TextFrame ActiveModRevision { get; } = new TextFrame();
        
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

        public string GameModeSelectedValue { get; set; } = "ThunderHawk";

        
        private bool inGameVisible = false;

        public bool InGameVisible
        {
            get { return inGameVisible; }

            set
            {
                inGameVisible = value;
                NotifyPropertyChanged("InGameVisible");
            }
        }


        private bool newsVisible = true;
        public bool NewsVisible{
            get
            {
                return newsVisible;
            }

            set
            {
                newsVisible = value;
                NotifyPropertyChanged("NewsVisible");
            }
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        
        public UriFrame Map { get; } = new UriFrame();
        
        
        public MainPageViewModel()
        {
            TitleButton.Text = CoreContext.LangService.GetString("MainPage").ToUpperInvariant();
            Map.Uri = new Uri("pack://application:,,,/ThunderHawk;component/Resources/Images/Maps/default.jpg");
        }
    }
}
