using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WrapperValueObject.Tests
{
	[WrapperValueObject(typeof(int))]
	readonly partial struct ValidateSingleId
	{
		static partial void Validate(int value)
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(
					nameof(value),
					"Id values cannot be less than 0.");
		}
	}

	[WrapperValueObject("Id", typeof(int), "Double1", typeof(double))]
	readonly partial struct ValidateMultipleId
	{
		static partial void Validate(int id, double double1)
		{
			if (id <= 0)
				throw new ArgumentOutOfRangeException(
					nameof(id),
					"Id values cannot be less than 0.");
			if (double.IsNaN(double1))
				throw new ArgumentException(
					"double1 values cannot be NaN", 
					nameof(double1));
		}
	}

	public class ValidateTests
	{
		[Fact]
		public void ValidateSingleThrowsExceptionLessThanZero()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => (ValidateSingleId)int.MinValue);
			Assert.Throws<ArgumentOutOfRangeException>(
				() => new ValidateSingleId(0));
		}

		[Fact]
		public void ValidateSingleSucceedsOnPositive()
		{
			var v1 = (ValidateSingleId)1;
			Assert.Equal(1, v1);
			var v2 = new ValidateSingleId(int.MaxValue);
			Assert.Equal(int.MaxValue, v2);
		}

		[Fact]
		public void ValidateMultipleThrowsExceptionLessThanZero()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => (ValidateMultipleId)(int.MinValue, double.NaN));
			Assert.Throws<ArgumentOutOfRangeException>(
				() => new ValidateMultipleId((0, 0d)));
			Assert.Throws<ArgumentException>(
				() => (ValidateMultipleId)(1, double.NaN));
			Assert.Throws<ArgumentException>(
				() => new ValidateMultipleId(1, double.NaN));
		}

		[Fact]
		public void ValidateMultipleSucceedsOnPositive()
		{
			var v1 = (ValidateMultipleId)(1, 2d);
			Assert.Equal((1, 2d), v1);
			var v2 = new ValidateMultipleId(int.MaxValue, double.MinValue);
			Assert.Equal((int.MaxValue, double.MinValue), v2);
		}
	}
}
