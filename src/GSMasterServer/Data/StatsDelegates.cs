namespace GSMasterServer.Data
{
    class StatsDelegates
    {
        internal static void Score3v3Updated(StatsData state, long value)
        {
            state.Score3v3 = value;
        }

        internal static long Score3v3Selector(StatsData state)
        {
            return state.Score3v3;
        }

        internal static void Score2v2Updated(StatsData state, long value)
        {
            state.Score2v2 = value;
        }

        internal static long Score2v2Selector(StatsData state)
        {
            return state.Score2v2;
        }

        internal static void Score1v1Updated(StatsData state, long value)
        {
            state.Score1v1 = value;
        }

        internal static long Score1v1Selector(StatsData state)
        {
            return state.Score1v1;
        }
    }
}
