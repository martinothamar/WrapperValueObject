using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WrapperValueObject.Tests
{
	[WrapperValueObject(typeof(int))]
	readonly partial struct ValidateId
	{
		static partial void Validate(int value)
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(
					nameof(value),
					"Id values cannot be less than 0.");
		}
	}

	public class ValidateTests
	{
		[Fact]
		public void ValidateThrowsExceptionLessThanZero()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => (ValidateId)int.MinValue);
			Assert.Throws<ArgumentOutOfRangeException>(
				() => new ValidateId(0));
		}

		[Fact]
		public void ValidateSucceedsOnPositive()
		{
			var v1 = (ValidateId)1;
			Assert.Equal(1, v1);
			var v2 = new ValidateId(int.MaxValue);
			Assert.Equal(int.MaxValue, v2);
		}
	}
}
