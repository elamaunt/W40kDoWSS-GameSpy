﻿namespace ThunderHawk.Core
{
    public interface IThunderHawkModManager
    {
        
        string JBugfixModName { get; }
        string BattleRoyaleModName { get; }
        string ValidModName { get; }
        string ValidModVersion { get; }
        string CurrentModName { get; set; }
        string CurrentModVersion{ get; set; }
        void DeployModAndModule(string gamePath, string modeNmae);
    }
}
