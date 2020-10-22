using System;
using Xunit;

namespace WrapperValueObject.Tests
{
    [WrapperValueObject] readonly partial struct ProductId { }

    public class ProductIdTypeTests
    {
        [Fact]
        public void Test_New()
        {
            var id = ProductId.New();

            Assert.NotEqual(Guid.Empty, (Guid)id);

            var id2 = id;

            Assert.Equal(id2, id);
            Assert.True(id2 == id);
            Assert.NotEqual(ProductId.New(), id);
            Assert.True(ProductId.New() != id);
        }
    }
}
