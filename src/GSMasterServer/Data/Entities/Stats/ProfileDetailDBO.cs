using LiteDB;

namespace GSMasterServer.Data
{
    public class ProfileDetailDBO
    {

        public long Id { get; set; }
        public long ProfileId { get; set; }
        
        public long SmGamesCount { get; set; }
        public long CsmGamesCount { get; set; }
        public long OrkGamesCount { get; set; }
        public long EldarGamesCount { get; set; }
        public long IgGamesCount { get; set; }
        public long NecGamesCount { get; set; }
        public long TauGamesCount { get; set; }
        public long DeGamesCount { get; set; }
        public long SobGamesCount { get; set; }

        public long SmWinCount { get; set; }
        
        public long CsmWinCount { get; set; }
        public long OrkWinCount { get; set; }
        public long EldarWinCount { get; set; }
        public long IgWinCount { get; set; }
        public long NecrWinCount { get; set; }
        public long TauWinCount { get; set; }
        public long DeWinCount { get; set; }
        public long SobWinCount { get; set; }
        
        public long SmGamesCountAuto { get; set; }
        public long CsmGamesCountAuto { get; set; }
        public long OrkGamesCountAuto { get; set; }
        public long EldarGamesCountAuto { get; set; }
        public long IgGamesCountAuto { get; set; }
        public long NecrGamesCountAuto { get; set; }
        public long TauGamesCountAuto { get; set; }
        public long DeGamesCountAuto { get; set; }
        public long SobGamesCountAuto { get; set; }

        public long SmWinCountAuto { get; set; }
        public long CsmWinCountAuto { get; set; }
        public long OrkWinCountAuto { get; set; }
        public long EldarWinCountAuto { get; set; }
        public long IgWinCountAuto { get; set; }
        public long NecrWinCountAuto { get; set; }
        public long TauWinCountAuto { get; set; }
        public long DeWinCountAuto { get; set; }
        public long SobWinCountAuto { get; set; }

        public ProfileDetailDBO()
        {
            
        }
        
        public ProfileDetailDBO(long profileId)
        {
            ProfileId = profileId;
        }

        public ProfileDetailDBO UpdateRaceStat(Race raceType, bool isWin, bool isAuto)
        {
            switch (raceType)
            {
                case Race.space_marine_race:
                    if (isAuto) SmGamesCountAuto++; else SmGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) SmWinCountAuto++; else SmWinCount++;
                    }
                    break;
                case Race.chaos_marine_race:
                    if (isAuto) CsmGamesCountAuto++; else CsmGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) CsmWinCountAuto++; else CsmWinCount++;
                    }
                    break;
                case Race.ork_race:
                    if (isAuto) OrkGamesCountAuto++; else OrkGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) OrkWinCountAuto++; else OrkWinCount++;
                    }
                    break;
                case Race.eldar_race:
                    if (isAuto) EldarGamesCountAuto++; else EldarGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) EldarWinCountAuto++; else EldarWinCount++;
                    }
                    break;
                case Race.guard_race:
                    if (isAuto) IgGamesCountAuto++; else IgGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) IgWinCountAuto++; else IgWinCount++;
                    }
                    break;
                case Race.necron_race:
                    if (isAuto) NecrGamesCountAuto++; else NecGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) NecrWinCountAuto++; else NecrWinCount++;
                    }
                    break;
                case Race.tau_race:
                    if (isAuto) TauGamesCountAuto++; else TauGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) TauWinCountAuto++; else TauWinCount++;
                    }
                    break;
                case Race.dark_eldar_race:
                    if (isAuto) DeGamesCountAuto++; else DeGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) DeWinCountAuto++; else DeWinCount++;
                    }
                    break;
                case Race.sisters_race:
                    if (isAuto) SobGamesCountAuto++; else SobGamesCount++;
                    if (isWin)
                    {
                        if (isAuto) SobWinCountAuto++; else SobWinCount++;
                    }
                    break;
            }

            return this;
        }
    }

    public class Profile1X1DBO : ProfileDetailDBO
    {

        public Profile1X1DBO() : base()
        {
            
        }
        public Profile1X1DBO(long profileId): base(profileId)
        {
        }
    }
    
    public class Profile2X2DBO : ProfileDetailDBO
    {
        public Profile2X2DBO() : base()
        {
            
        }
        public Profile2X2DBO(long profileId) : base(profileId)
        {
        }
    }
    public class Profile3X3DBO : ProfileDetailDBO
    {
        public Profile3X3DBO() : base()
        {
            
        }
        public Profile3X3DBO(long profileId) : base(profileId)
        {
        }
    }
    public class Profile4X4DBO : ProfileDetailDBO
    {
        public Profile4X4DBO() : base()
        {
            
        }
        public Profile4X4DBO(long profileId) : base(profileId)
        {
        }
    }
}