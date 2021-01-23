using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WrapperValueObject.Generator
{
	[Generator]
	public partial class Generator : ISourceGenerator
	{
		private static readonly DiagnosticDescriptor NoNestingRule = new DiagnosticDescriptor(
			"WVOG00001",
			"Target types for WrapperValueObject generator can't be nested within other types",
			"Target types for WrapperValueObject generator can't be nested within other types",
			typeof(Generator).FullName,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);
		private static readonly DiagnosticDescriptor MissingPartialModifierRule = new DiagnosticDescriptor(
			"WVOG00002",
			"Target types for WrapperValueObject generator must be partial",
			"Target types for WrapperValueObject generator must be partial",
			typeof(Generator).FullName,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public void Execute(GeneratorExecutionContext context)
		{
			GenerateAttribute(context);

			var compilation = context.Compilation;

			var sourceBuilder = new StringBuilder();

			foreach (var tree in compilation.SyntaxTrees)
			{
				var semanticModel = compilation.GetSemanticModel(tree);

				foreach (var node in tree.GetRoot().DescendantNodesAndSelf().OfType<AttributeListSyntax>())
				{
					ProcessAttributeList(context, node, semanticModel, sourceBuilder);

					sourceBuilder.Clear();
				}
			}

			void ProcessAttributeList(GeneratorExecutionContext context, AttributeListSyntax attributeListNode, SemanticModel semanticModel, StringBuilder sourceBuilder)
			{
				// System.Diagnostics.Debugger.Launch();

				var attributeNode = attributeListNode.Attributes.SingleOrDefault(a => a.Name.ToString() == "WrapperValueObject");
				if (attributeNode is null)
					return;

				var node = (TypeDeclarationSyntax)attributeListNode.Parent!;

				if (node.Parent is ClassDeclarationSyntax)
				{
					context.ReportDiagnostic(Diagnostic.Create(NoNestingRule, node.GetLocation()));
					return;
				}

				if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
				{
					context.ReportDiagnostic(Diagnostic.Create(MissingPartialModifierRule, node.GetLocation()));
					return;
				}

				var type = semanticModel.GetDeclaredSymbol(node);

				List<(string Name, INamedTypeSymbol Type)> innerTypes = new();

				var currentName = "Value";
				bool generateImplicitConversionToPrimitive = false;
				bool? generateComparisonOperators = default;
				bool? generateMathOperators = default;

				if (attributeNode.ArgumentList?.Arguments is null)
				{
					innerTypes.Add(("Value", context.Compilation.GetTypeByMetadataName("System.Guid")!));
				}
				else
				{
					var flag = false;

					foreach (var a in attributeNode.ArgumentList.Arguments)
					{
						var expression = a.Expression;

						if (expression is TypeOfExpressionSyntax typeOfExpression)
						{
							// Single type
							flag = true;
							var typeSymbol = semanticModel.GetSymbolInfo(typeOfExpression.Type).Symbol;
							innerTypes.Add((currentName!, (INamedTypeSymbol)typeSymbol!));
							currentName = "Value";
						}
						else if (a.NameEquals != null)
						{
							var id = a.NameEquals.Name.Identifier;

							switch (id.ValueText)
							{
								case "GenerateImplicitConversionToPrimitive":
									generateImplicitConversionToPrimitive = expression.Kind() == SyntaxKind.TrueLiteralExpression;
									break;

								case "GenerateComparisonOperators":
									generateComparisonOperators =
										((MemberAccessExpressionSyntax)expression).Name.Identifier.ValueText switch
										{
											"True" => true,
											"False" => false,
											_ => default(bool?),
										};
									break;

								case "GenerateMathOperators":
									generateMathOperators =
										((MemberAccessExpressionSyntax)expression).Name.Identifier.ValueText switch
										{
											"True" => true,
											"False" => false,
											_ => default(bool?),
										};
									break;

							}
						}
						else
						{
							// Value tuple config
							currentName = (string)semanticModel.GetConstantValue(expression).Value!;
						}
					}

					if (!flag)
						innerTypes.Add(("Value", context.Compilation.GetTypeByMetadataName("System.Guid")!));
				}

				var generationContext = new GenerationContext(
					context,
					sourceBuilder,
					node,
					type!,
					innerTypes,
					generateImplicitConversionToPrimitive,
					generateComparisonOperators,
					generateMathOperators);

				_ = GenerateWrapper(in generationContext);
			}
		}

		private void GenerateAttribute(GeneratorExecutionContext context)
		{
			var attributeSource = EmbeddedResource.GetContent(@"resources\WrapperValueObjectAttribute.cs");
			context.AddSource("WrapperValueObjectAttribute.cs", SourceText.From(attributeSource, Encoding.UTF8));
		}

		private static readonly string[] MathTypes = new[]
		{
			"System.SByte",
			"System.Byte",
			"System.Int16",
			"System.UInt16",
			"System.Int32",
			"System.UInt32",
			"System.Int64",
			"System.UInt64",
			"System.Single",
			"System.Double",
			"System.Decimal",
		};

		private static readonly string[] ByteTypes = new[]
		{
			"System.SByte",
			"System.Byte",
		};

		private bool GenerateWrapper(in GenerationContext context)
		{
			var isReadOnly = context.Node.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

			var innerType = string.Empty;
			var isSingleType = context.InnerTypes.Count() == 1;
			var isDefaultIdCase = false;

			var implicitConversion = context.GenerateImplicitConversionToPrimitive;
			var comparison = context.GenerateComparisonOperators;
			var math = context.GenerateMathOperators;
			var isNumericType = false;

			if (isSingleType)
			{
				// If we have 1 type, we might be able to safely generate math operations
				var singleType = context.InnerTypes.Single();
				innerType = $"{singleType.Type!.ContainingNamespace}.{singleType.Type.Name}";
				isDefaultIdCase = innerType == "System.Guid";

				if (MathTypes!.Contains(innerType))
				{
					isNumericType = true;
					math ??= !context.Type.Name.EndsWith("Id");
					comparison ??= !context.Type.Name.EndsWith("Id");
				}
				else
				{
					comparison = math = false;
				}
			}
			else
			{
				innerType = context.InnerTypes.Count() == 1 ? "" : $"({string.Join(", ", context.InnerTypes.Select(t => $"{t.Type.ContainingNamespace}.{t.Type.Name}"))})";
				comparison = math = false;
			}

			var hasToString = context.Node.Members
				.Where(m => m.IsKind(SyntaxKind.MethodDeclaration))
				.Cast<MethodDeclarationSyntax>()
				.Any(m => m.Modifiers.Any(mo => mo.IsKind(SyntaxKind.OverrideKeyword)) && m.Identifier.Text == "ToString" && !m.ParameterList.Parameters.Any());

			context.SourceBuilder.AppendLine(@$"
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace {context.Type.ContainingNamespace}
{{ 
    partial {(context.Node is ClassDeclarationSyntax ? "class" : "struct")} {context.Type.Name} : IEquatable<{context.Type.Name}>, IComparable<{context.Type.Name}>
    {{
        private readonly {innerType} _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {context.Type.Name}({innerType} other)
        {{
            _value = other;
        }}
");

			if (!isSingleType)
			{
				context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {context.Type.Name}({string.Join(", ", context.InnerTypes.Select((t, i) => $"{t.Type.ContainingNamespace}.{t.Type.Name} {t.Name.FirstCharToLower()}"))})
        {{
            _value = ({string.Join(", ", context.InnerTypes.Select((t, i) => t.Name.FirstCharToLower()))});
        }}
");

			}

			context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {context.Type.Name}({(isReadOnly ? "in " : "")}{context.Type.Name} other)
        {{
            _value = other._value;
        }}

");

			if (isDefaultIdCase)
			{
				context.SourceBuilder.AppendLine(@$"
        public static {context.Type.Name} New() => Guid.NewGuid();
");
			}

			if (isSingleType)
			{
				context.SourceBuilder.AppendLine(@$"
        public readonly {innerType} {context.InnerTypes.Single().Name} => _value;
");
			}
			else
			{
				for (int i = 0; i < context.InnerTypes.Count(); i++)
				{
					var t = context.InnerTypes.ElementAt(i);
					context.SourceBuilder.AppendLine(@$"
        public readonly {t.Type.ContainingNamespace}.{t.Type.Name} {t.Name} => _value.Item{i + 1};
");
				}
			}

			context.SourceBuilder.AppendLine(@$"

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator {context.Type.Name}({innerType} other) => new {context.Type.Name}(other);");

			if (implicitConversion)
			{
				context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator {innerType}({context.Type.Name} other) => other._value;");
			}
			else
			{
				context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator {innerType}({context.Type.Name} other) => other._value;");
			}

			context.SourceBuilder.AppendLine(@$"

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => _value.Equals(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals({context.Type.Name} obj) => _value.Equals(obj._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo({context.Type.Name} value) => _value.CompareTo(value._value);

");

			if (isNumericType)
			{
				context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(IFormatProvider? provider) => _value.ToString(provider);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string? format) => _value.ToString(format);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string? format, IFormatProvider? provider) => _value.ToString(format, provider);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
            => _value.TryFormat(destination, out charsWritten, format, provider);");
			}

			if (math == true)
			{
				var isByteType = ByteTypes.Contains(innerType);

				if (isByteType)
				{
					context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value + right._value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value - right._value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value / right._value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value * right._value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value % right._value;
                    ");
				}
				else
				{
					context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {context.Type.Name} operator +({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => new {context.Type.Name}(left._value + right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {context.Type.Name} operator -({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => new {context.Type.Name}(left._value - right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {context.Type.Name} operator /({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => new {context.Type.Name}(left._value / right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {context.Type.Name} operator *({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => new {context.Type.Name}(left._value * right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {context.Type.Name} operator %({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => new {context.Type.Name}(left._value % right._value);
                    ");
				}

			}

			if (comparison == true)
			{
				if (isNumericType)
				{
					context.SourceBuilder.AppendLine(@$"

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value >= right._value;
");
				}
				else
				{
					context.SourceBuilder.AppendLine(@$"

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value.CompareTo(right._value) == -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value.CompareTo(right._value) == 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right)
        {{
            var result = left._value.CompareTo(right._value);
            return result == -1 || result == 0;
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right)
        {{
            var result = left._value.CompareTo(right._value);
            return result == 1 || result == 0;
        }}
");
				}
			}

			if (!hasToString)
			{

				context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => _value.ToString();
");
			}

			if (isReadOnly)
			{
				context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in {context.Type.Name} obj)
        {{
            return _value.Equals(obj._value);
        }}
");
			}

			context.SourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=({(isReadOnly ? "in " : "")}{context.Type.Name} left, {(isReadOnly ? "in " : "")}{context.Type.Name} right) => left._value != right._value;
    }}");

			context.SourceBuilder.AppendLine(@$"
}}");

			context.Context.AddSource($"{context.Type.Name}_Implementation.cs", SourceText.From(context.SourceBuilder.ToString(), Encoding.UTF8));
			return true;

			static void GenerateSystemTextJsonConverter(
				GeneratorExecutionContext context,
				StringBuilder sourceBuilder,
				TypeDeclarationSyntax node,
				ISymbol type,
				IEnumerable<(string Name, INamedTypeSymbol Type)> innerTypes
			)
			{

				sourceBuilder.AppendLine(@$"
    public sealed class {type.Name}JsonConverter
");
			}
		}

		public void Initialize(GeneratorInitializationContext context)
		{
		}
	}
}
