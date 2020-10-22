using Xunit;

namespace WrapperValueObject.Tests
{
    [WrapperValueObject(typeof(double))]
    public readonly partial struct Probability
    {
    }

    public class ProbabilityTypeTests
    {
        [Fact]
        public void Test_Add()
        {
            Probability probability = 0.9;

            var result = probability + 0.05;
            var result2 = probability + new Probability(0.05);

            Assert.True(result == result2);
            Assert.Equal(((double)probability) + 0.05, (double)result);
            Assert.True(probability != result);
            Assert.True(probability == 0.9);
        }

        [Fact]
        public void Test_Subtract()
        {
            Probability probability = 0.9;

            var result = probability - 0.05;
            var result2 = probability - new Probability(0.05);

            Assert.True(result == result2);
            Assert.Equal(((double)probability) - 0.05, (double)result);
            Assert.True(probability != result);
            Assert.True(probability == 0.9);
        }

        [Fact]
        public void Test_Multiply()
        {
            Probability probability = 0.9;

            var result = probability * 0.05;
            var result2 = probability * new Probability(0.05);

            Assert.True(result == result2);
            Assert.Equal(((double)probability) * 0.05, (double)result);
            Assert.True(probability != result);
            Assert.True(probability == 0.9);
        }

        [Fact]
        public void Test_Divide()
        {
            Probability probability = 0.9;

            var result = probability / 0.05;
            var result2 = probability / new Probability(0.05);

            Assert.True(result == result2);
            Assert.Equal(((double)probability) / 0.05, (double)result);
            Assert.True(probability != result);
            Assert.True(probability == 0.9);
        }

        [Fact]
        public void Test_Modulo()
        {
            Probability probability = 0.9;

            var result = probability % 0.05;
            var result2 = probability % new Probability(0.05);

            Assert.True(result == result2);
            Assert.Equal(((double)probability) % 0.05, (double)result);
            Assert.True(probability != result);
            Assert.True(probability == 0.9);
        }
    }
}
