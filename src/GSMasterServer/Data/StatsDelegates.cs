namespace GSMasterServer.Data
{
    class StatsDelegates
    {
        internal static void Score3v3Updated(ProfileDBO state, long value)
        {
            state.Score3v3 = value;
        }

        internal static long Score3v3Selector(ProfileDBO state)
        {
            return state.Score3v3;
        }

        internal static void Score2v2Updated(ProfileDBO state, long value)
        {
            state.Score2v2 = value;
        }

        internal static long Score2v2Selector(ProfileDBO state)
        {
            return state.Score2v2;
        }

        internal static void Score1v1Updated(ProfileDBO state, long value)
        {
            state.Score1v1 = value;
        }

        internal static long Score1v1Selector(ProfileDBO state)
        {
            return state.Score1v1;
        }
    }
}
