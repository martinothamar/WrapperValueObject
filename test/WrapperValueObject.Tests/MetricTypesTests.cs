using Xunit;

namespace WrapperValueObject.Tests
{
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

    public class MetricTypesTests
    {
        [Fact]
        public void Test_Conversion()
        {
            MeterLength meters = 2;

            CentimeterLength centiMeters = meters;

            Assert.Equal(200, (int)centiMeters);
        }

        [Fact]
        public void Test_Add()
        {
            MeterLength meters = 2;

            var result = meters + 2;

            Assert.Equal(((int)meters) + 2, (int)result);
            Assert.True(meters != result);
            Assert.True(meters == 2);
        }

        [Fact]
        public void Test_Subtract()
        {
            MeterLength meters = 5;

            var result = meters - 2;

            Assert.Equal(((int)meters) - 2, (int)result);
            Assert.True(meters != result);
            Assert.True(meters == 5);
        }

        [Fact]
        public void Test_Multiply()
        {
            MeterLength meters = 5;

            var result = meters * 2;

            Assert.Equal(((int)meters) * 2, (int)result);
            Assert.True(meters != result);
            Assert.True(meters == 5);
        }

        [Fact]
        public void Test_Divide()
        {
            MeterLength meters = 2;

            var result = meters / 2;

            Assert.Equal(((int)meters) / 2, (int)result);
            Assert.True(meters != result);
            Assert.True(meters == 2);
        }
    }
}
