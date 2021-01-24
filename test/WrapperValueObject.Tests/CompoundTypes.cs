using System;
using Xunit;

namespace WrapperValueObject.Tests
{
    [WrapperValueObject("MatchId", typeof(MatchId), "HomeGoals", typeof(byte), "AwayGoals", typeof(byte))]
    public readonly partial struct MatchResult
    {
    }

    public class CompoundTypes
    {
        [Fact]
        public void Test_Compound_Type_Equality()
        {
            MatchId id = 3;

            MatchResult result = (id, 1, 2);

            Assert.Equal(new MatchResult(id, 1, 2), result);
            Assert.Equal(id, result.MatchId);
            Assert.Equal(1, result.HomeGoals);
            Assert.Equal(2, result.AwayGoals);
        }
    }
}
