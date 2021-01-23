using System;
using Xunit;

namespace WrapperValueObject.Tests
{
	[WrapperValueObject(typeof(int))]
	public readonly partial struct LeagueId { }

	[WrapperValueObject(typeof(int), GenerateImplicitConversionToPrimitive = true)]
	public readonly partial struct MatchId { }

	[WrapperValueObject(typeof(int), GenerateComparisonOperators = TriBool.True, GenerateMathOperators = TriBool.True)]
	public readonly partial struct ScoreId { }

	[WrapperValueObject(typeof(int), GenerateComparisonOperators = TriBool.False, GenerateMathOperators = TriBool.False)]
	public readonly partial struct NonScore { }

	public class IdTypes
	{
		[Fact]
		public void Test_Basic_Id_Int_Type()
		{
			LeagueId id1 = 1;
			LeagueId id2 = 2;
			//LeagueId id3 = id1 + id2;

			Assert.NotEqual(id1, id2);
			//Assert.Equal((LeagueId)3, id3);

			//Assert.True(id1 < id2);
			//Assert.True(id1 <= id2);
			//Assert.False(id1 > id2);
			//Assert.False(id1 >= id2);
		}

		[Fact]
		public void Test_Id_Int_Type_With_Implicit_Conversion()
		{
			MatchId id1 = 1;
			MatchId id2 = 2;
			MatchId id3 = id1 + id2;

			Assert.NotEqual(id1, id2);
			Assert.Equal((MatchId)3, id3);

			Assert.True(id1 < id2);
			Assert.True(id1 <= id2);
			Assert.False(id1 > id2);
			Assert.False(id1 >= id2);
		}

		[Fact]
		public void Test_Id_Int_Type_With_Arguments()
		{
			ScoreId id1 = 1;
			ScoreId id2 = 2;
			ScoreId id3 = id1 + id2;

			Assert.NotEqual(id1, id2);
			Assert.Equal((ScoreId)3, id3);

			Assert.True(id1 < id2);
			Assert.True(id1 <= id2);
			Assert.False(id1 > id2);
			Assert.False(id1 >= id2);
		}

		[Fact]
		public void Test_Non_Id_Int_Type_With_Arguments()
		{
			NonScore id1 = 1;
			NonScore id2 = 2;
			//NonScore id3 = id1 + id2;

			Assert.NotEqual(id1, id2);
			//Assert.Equal((NonScore)3, id3);

			//Assert.True(id1 < id2);
			//Assert.True(id1 <= id2);
			//Assert.False(id1 > id2);
			//Assert.False(id1 >= id2);
		}
	}
}
