using System;
using System.Diagnostics;

namespace WrapperValueObject.TestConsole
{
    [WrapperValueObject]
    public readonly partial struct MatchId
    {
    }

    [WrapperValueObject("HomeGoals", typeof(byte), "AwayGoals", typeof(byte))]
    public readonly partial struct MatchResult
    {
    }

    public partial struct Match
    {
        public readonly MatchId MatchId { get; }

        public MatchResult Result { get; private set; }

        public void SetResult(MatchResult result) => Result = result;

        public Match(in MatchId matchId)
        {
            MatchId = matchId;
            Result = default;
        }
    }

    public static class Program
    {
        static void Main()
        {
            var match = new Match(MatchId.New());

            match.SetResult((1, 2));
            match.SetResult(new MatchResult(1, 2));

            var otherResult = new MatchResult(2, 1);

            Debug.Assert(otherResult != match.Result);

            match.SetResult((2, 1));
            Debug.Assert(otherResult == match.Result);

            Debug.Assert(match.MatchId != default);
            Debug.Assert(match.Result != default);
            Debug.Assert(match.Result.HomeGoals == 2);
            Debug.Assert(match.Result.AwayGoals == 1);
        }
    }
}
