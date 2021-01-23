using Xunit;

namespace WrapperValueObject.Tests
{
    [WrapperValueObject(typeof(byte))]
    public readonly partial struct Score
    {
    }

    public class ScoreTypeTests
    {
        [Fact]
        public void Test_Add()
        {
            Score probability = 10;

            var result = probability + 5;
            var result10 = probability + new Score(5);

            Assert.True(result == result10);
            Assert.Equal(((byte)probability) + 5, (byte)result);
            Assert.True((byte)probability != result);
            Assert.True((byte)probability == 10);
        }

        [Fact]
        public void Test_Subtract()
        {
            Score probability = 10;

            var result = probability - 5;
            var result10 = probability - new Score(5);

            Assert.True(result == result10);
            Assert.Equal(((byte)probability) - 5, (byte)result);
            Assert.True((byte)probability != result);
            Assert.True((byte)probability == 10);
        }

        [Fact]
        public void Test_Multiply()
        {
            Score probability = 10;

            var result = probability * 5;
            var result10 = probability * new Score(5);

            Assert.True(result == result10);
            Assert.Equal(((byte)probability) * 5, (byte)result);
            Assert.True((byte)probability != result);
            Assert.True((byte)probability == 10);
        }

        [Fact]
        public void Test_Divide()
        {
            Score probability = 10;

            var result = probability / 5;
            var result10 = probability / new Score(5);

            Assert.True(result == result10);
            Assert.Equal(((byte)probability) / 5, (byte)result);
            Assert.True((byte)probability != result);
            Assert.True((byte)probability == 10);
        }

        [Fact]
        public void Test_Modulo()
        {
            Score probability = 10;

            var result = probability % 5;
            var result10 = probability % new Score(5);

            Assert.True(result == result10);
            Assert.Equal(((byte)probability) % 5, (byte)result);
            Assert.True((byte)probability != result);
            Assert.True((byte)probability == 10);
        }
    }
}
