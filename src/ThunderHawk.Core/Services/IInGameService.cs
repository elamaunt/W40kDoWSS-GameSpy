using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SharedServices;

namespace ThunderHawk.Core
{
    public interface IInGameService
    {
        float apmCurrent{ get;}
        float apmAverageGame{ get; }
        
        bool isGameNow{ get;}
        
        string inGameMap { get; }

        ObservableCollection<ChatUserItemViewModel> serverOnlinePlayers { set;}
        
        InGamePlayer[] inGamePlayers { get; }

        void DropSsConsoleOffset();
    }

    public class InGamePlayer
    {
        public string Name;
        public bool IsLoadComplete;
        public long Mmr;
        public int Team;
        public Race Race;
    }
}
