using Xunit;

namespace WrapperValueObject.Tests
{
    [WrapperValueObject(typeof(decimal))]
    public readonly partial struct Money
    {
    }

    public class MoneyTypeTests
    {
        [Fact]
        public void Test_Add()
        {
            Money money = 2m;

            var result = money + 2m;
            var result2 = money + new Money(2m);

            Assert.True(result == result2);
            Assert.Equal(((decimal)money) + 2m, (decimal)result);
            Assert.True(money != result);
            Assert.True(money == 2m);
        }

        [Fact]
        public void Test_Subtract()
        {
            Money money = 5m;

            var result = money - 2m;

            Assert.Equal(((decimal)money) - 2m, (decimal)result);
            Assert.True(money != result);
            Assert.True(money == 5m);
        }

        [Fact]
        public void Test_Multiply()
        {
            Money money = 5m;

            var result = money * 2m;

            Assert.Equal(((decimal)money) * 2m, (decimal)result);
            Assert.True(money != result);
            Assert.True(money == 5m);
        }

        [Fact]
        public void Test_Divide()
        {
            Money money = 2m;

            var result = money / 2m;

            Assert.Equal(((decimal)money) / 2m, (decimal)result);
            Assert.True(money != result);
            Assert.True(money == 2m);
        }

        [Fact]
        public void Test_Modulo()
        {
            Money money = 2m;

            var result = money % 2m;

            Assert.Equal(((decimal)money) % 2m, (decimal)result);
            Assert.True(money != result);
            Assert.True(money == 2m);
        }
    }
}
