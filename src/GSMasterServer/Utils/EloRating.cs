using System;

namespace GSMasterServer.Utils
{
    public static class EloRating
    {
        public static double ExpectationToWin(long playerOneRating, long playerTwoRating)
        {
            return 1 / (1 + Math.Pow(10, (playerTwoRating - playerOneRating) / 400.0));
        }

        public enum GameOutcome
        {
            Win = 1,
            Loss = 0
        }

        public static long CalculateELOdelta(long teamOneRating, long teamTwoRating, GameOutcome gameResult)
        {
            long eloK = 32L;

            return (long)(eloK * ((long)gameResult - ExpectationToWin(teamOneRating, teamTwoRating)));
        }

        public static void ChangeByELO(ref long playerOneRating, ref long playerTwoRating, GameOutcome gameResult)
        {
            var delta = CalculateELOdelta(playerOneRating, playerTwoRating, gameResult);

            playerOneRating += delta;
            playerTwoRating -= delta;
        }
    }
}
