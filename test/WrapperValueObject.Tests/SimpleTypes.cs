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

    [WrapperValueObject(typeof(int))]
    public readonly partial struct MeterLength 
    {
        public static implicit operator CentimeterLength(MeterLength meter) => meter.Value * 100;
    }

    [WrapperValueObject(typeof(int))]
    public readonly partial struct CentimeterLength
    {
        public static implicit operator MeterLength(CentimeterLength centiMeter) => centiMeter.Value / 100;
    }

    public class SimpleTypes
    {
        [Fact]
        public void Test_Metric_Types()
        {
            MeterLength meters = 2;

            CentimeterLength centiMeters = meters;

            Assert.Equal(200, (int)centiMeters);
        }

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
