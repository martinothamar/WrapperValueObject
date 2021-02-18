using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WrapperValueObject.Generator
{
	public partial class Generator
	{
		private readonly struct GenerationContext
		{
			public readonly GeneratorExecutionContext Context;
			public readonly StringBuilder SourceBuilder;
			public readonly TypeDeclarationSyntax Node;
			public readonly ISymbol Type;
			public readonly IReadOnlyList<(string Name, INamedTypeSymbol Type)> InnerTypes;
			public readonly bool GenerateImplicitConversionToPrimitive;
			public readonly bool? GenerateComparisonOperators;
			public readonly bool? GenerateMathOperators;

			public GenerationContext(
				GeneratorExecutionContext context,
				StringBuilder sourceBuilder,
				TypeDeclarationSyntax node,
				ISymbol type,
				IReadOnlyList<(string Name, INamedTypeSymbol Type)> innerTypes,
				bool generateImplicitConversionToPrimitive,
				bool? generateComparisonOperators,
				bool? generateMathOperators
			)
			{
				Context = context;
				SourceBuilder = sourceBuilder;
				Node = node;
				Type = type;
				InnerTypes = innerTypes;
				GenerateImplicitConversionToPrimitive = generateImplicitConversionToPrimitive;
				GenerateComparisonOperators = generateComparisonOperators;
				GenerateMathOperators = generateMathOperators;
			}
		}
	}
}
