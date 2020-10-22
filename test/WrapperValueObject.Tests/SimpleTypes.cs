using System;
using Xunit;

namespace WrapperValueObject.Tests
{
    [WrapperValueObject(typeof(Guid))]
    public readonly partial struct MatchId
    {
    }

    [WrapperValueObject(typeof(int))]
    public readonly partial struct LeagueId
    {
    }

    public class SimpleTypes
    {
        [Fact]
        public void Test_Guid_Type_Equals()
        {
            var guid = Guid.NewGuid();
            MatchId val = guid;

            Assert.Equal((Guid)val, guid);
            Assert.True(val == guid);
            Assert.False(val != guid);
        }

        [Fact]
        public void Test_Int_Type_Comparison()
        {
            LeagueId id1 = 1;
            LeagueId id2 = 2;
            LeagueId id3 = id1 + id2;

            Assert.NotEqual(id1, id2);
            Assert.Equal((LeagueId)3, id3);

            Assert.True(id1 < id2);
            Assert.True(id1 <= id2);
            Assert.False(id1 > id2);
            Assert.False(id1 >= id2);
        }
    }
}
