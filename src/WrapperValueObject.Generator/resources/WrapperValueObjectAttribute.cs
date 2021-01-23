using System;

namespace WrapperValueObject
{
	[Flags]
	public enum WrapperValueObjectJsonConverter
	{
		None = 0,
		NewtonsoftJson = 1,
		SystemTextJson = 2,

		All = NewtonsoftJson | SystemTextJson,
	}

	public enum TriBool
	{
		Unset = 0,
		True = 1,
		False = 2,
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public class WrapperValueObjectAttribute : Attribute
	{
		private readonly (string Name, Type Type)[] _types;

		/// <summary>
		/// Whether or not to generate the <c>implicit</c> conversion back to
		/// an operator. By default, the implicit conversion will not be generated.
		/// </summary>
		public bool GenerateImplicitConversionToPrimitive { get; set; } = false;

		/// <summary>
		/// Explicitly express whether or not to generate comparison operators. 
		/// This will be ignored for <see cref="Guid"/> base types as well as more than one type.
		/// If left unset, Comparison operators will be generated as long as
		/// the containing <c>struct</c>'s name does not end with <c>Id</c>.
		/// </summary>
		public TriBool GenerateComparisonOperators { get; set; } = TriBool.Unset;

		/// <summary>
		/// Explicitly express whether or not to generate math operators. 
		/// This will be ignored for <see cref="Guid"/> base types as well as more than one type.
		/// If left unset, Comparison operators will be generated as long as
		/// the containing <c>struct</c>'s name does not end with <c>Id</c>.
		/// </summary>
		public TriBool GenerateMathOperators { get; set; } = TriBool.Unset;

		/*
		/// <summary>
		/// Which Json Converter(s) to generate, if any. By default, only the <c>System.Text.Json</c> generator
		/// will be rendered, but one can be generated for <c>Newtonsoft.Json</c>.
		/// </summary>
		public WrapperValueObjectJsonConverter JsonConverter { get; set; } = WrapperValueObjectJsonConverter.SystemTextJson;
		*/

		public WrapperValueObjectAttribute()
			: this(typeof(Guid))
		{
		}

		public WrapperValueObjectAttribute(Type type)
		{
			_types = new (string Name, Type Type)[] { ("Value", type) };
		}

		public WrapperValueObjectAttribute(string name1, Type type1)
		{
			_types = new (string Name, Type Type)[]
			{
				(name1, type1),
			};
		}

		public WrapperValueObjectAttribute(string name1, Type type1, string name2, Type type2)
		{
			_types = new (string Name, Type Type)[]
			{
				(name1, type1),
				(name2, type2),
			};
		}

		public WrapperValueObjectAttribute(string name1, Type type1, string name2, Type type2, string name3, Type type3)
		{
			_types = new (string Name, Type Type)[]
			{
				(name1, type1),
				(name2, type2),
				(name3, type3),
			};
		}

		public WrapperValueObjectAttribute(string name1, Type type1, string name2, Type type2, string name3, Type type3, string name4, Type type4)
		{
			_types = new (string Name, Type Type)[]
			{
				(name1, type1),
				(name2, type2),
				(name3, type3),
				(name4, type4),
			};
		}
	}
}
