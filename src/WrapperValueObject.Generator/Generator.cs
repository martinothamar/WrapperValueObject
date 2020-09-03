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
    public class Generator : ISourceGenerator
    {
        public void Execute(SourceGeneratorContext context)
        {
            GenerateAttribute(context);

            var compilation = context.Compilation;

            var sourceBuilder = new StringBuilder();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);
                
                foreach (var node in tree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>())
                {
                    var isClassOrStruct = node is ClassDeclarationSyntax || node is StructDeclarationSyntax;
                    if (!isClassOrStruct)
                        continue;

                    if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                        continue;

                    if (!node.AttributeLists.Any())
                        continue;

                    //System.Diagnostics.Debugger.Launch();

                    var type = semanticModel.GetDeclaredSymbol(node);
                    var attribute = node!.AttributeLists
                        .SelectMany(al => al.Attributes.Where(a => a.Name.ToString() == "WrapperValueObject"))
                        .Where(a => a != null)
                        .SingleOrDefault();
                    if (attribute is null)
                        continue;

                    if (attribute.ArgumentList!.Arguments.Count == 1)
                    {
                        var wrapperType = (INamedTypeSymbol)semanticModel
                            .GetSymbolInfo(((TypeOfExpressionSyntax)attribute.ArgumentList!.Arguments[0].Expression).Type).Symbol!;

                        var csharpType = GenerateWrapper(context, sourceBuilder, node, type!, $"{wrapperType!.ContainingNamespace}.{wrapperType.Name}");
                    }
                    else
                    {

                        List<(string Name, INamedTypeSymbol Type)> innerTypes = new();

                        var currentName = "Value";

                        foreach (var a in attribute.ArgumentList.Arguments)
                        {
                            var expression = a.Expression;

                            if (expression is TypeOfExpressionSyntax typeOfExpression)
                            {
                                // Single type
                                var typeSymbol = semanticModel.GetSymbolInfo(typeOfExpression.Type).Symbol;
                                innerTypes.Add((currentName, (INamedTypeSymbol)typeSymbol!));
                                currentName = "Value";
                            }
                            else
                            {
                                // Value tuple config
                                currentName = (string)semanticModel.GetConstantValue(expression).Value;
                            }
                        }

                        GenerateWrapper(context, sourceBuilder, node, type!, innerTypes);
                    }

                    sourceBuilder.Clear();
                }
            }
        }

        private void GenerateAttribute(SourceGeneratorContext context)
        {
            string attributeSource = @"
using System;
    
namespace WrapperValueObject
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class WrapperValueObjectAttribute : Attribute
    {
        private readonly (string Name, Type Type)[] _types;

        public WrapperValueObjectAttribute(Type type)
        {
            _types = new (string Name, Type Type)[] { (""Value"", type) };
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
    }
}
";

            context.AddSource("Attribute", SourceText.From(attributeSource, Encoding.UTF8));
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

        private bool GenerateWrapper(SourceGeneratorContext context, StringBuilder sourceBuilder, TypeDeclarationSyntax node, ISymbol type, string innerType)
        {
            var isReadOnly = node.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

            // TODO - options for methods to generate, don't generate methods where the user has already supplied them (i.e. custom ToString)

            var isMath = MathTypes.Contains(innerType);

            var hasToString = node.Members
                .Where(m => m.IsKind(SyntaxKind.MethodDeclaration))
                .Cast<MethodDeclarationSyntax>()
                .Any(m => m.Modifiers.Any(mo => mo.IsKind(SyntaxKind.OverrideKeyword)) && m.Identifier.Text == "ToString" && !m.ParameterList.Parameters.Any());

            sourceBuilder.AppendLine(@$"
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace {type.ContainingNamespace}
{{ 
    partial {(node is ClassDeclarationSyntax ? "class" : "struct")} {type.Name} : IEquatable<{type.Name}>, IComparable<{type.Name}>, IComparable
    {{
        private readonly {innerType} _value; // Do not rename (binary serialization)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {type.Name}({innerType} other)
        {{
            _value = other;
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {type.Name}({(isReadOnly ? "in " : "")}{type.Name} other)
        {{
            _value = other._value;
        }}

");
            sourceBuilder.AppendLine(@$"

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator {innerType}({type.Name} other) => other._value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator {type.Name}({innerType} other) => new {type.Name}(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => _value.Equals(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals({type.Name} obj) => _value.Equals(obj._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object? value) => _value.CompareTo(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo({type.Name} value) => _value.CompareTo(value._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value != right._value;
");

            if (isMath)
            {
//                sourceBuilder.AppendLine(@$"
//");
                sourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(IFormatProvider? provider) => _value.ToString(provider);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string? format) => _value.ToString(format);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string? format, IFormatProvider? provider) => _value.ToString(format, provider);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
            => _value.TryFormat(destination, out charsWritten, format, provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator +({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left._value + right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator -({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left._value - right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator /({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left._value / right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator *({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left._value * right._value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator %({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left._value % right._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value >= right._value;
");
            }
            else
            {
                sourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value.CompareTo(right._value) == -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value.CompareTo(right._value) == 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right)
        {{
            var result = left._value.CompareTo(right._value);
            return result == -1 || result == 0;
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right)
        {{
            var result = left._value.CompareTo(right._value);
            return result == 1 || result == 0;
        }}
");
            }

            if (!hasToString)
            {

				sourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => _value.ToString();
");
			}

            if (isReadOnly)
            {
                sourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in {type.Name} obj)
        {{
            return _value.Equals(obj._value);
        }}
");
            }

            sourceBuilder.AppendLine(@$"
    }}
}}");


            context.AddSource($"{type.Name}_Implementation.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            return true;
        }

        private bool GenerateWrapper(SourceGeneratorContext context, StringBuilder sourceBuilder, TypeDeclarationSyntax node, ISymbol type, IEnumerable<(string Name, INamedTypeSymbol Type)> innerTypes)
        {
            var isReadOnly = node.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

            var innerType = $"({string.Join(", ", innerTypes.Select(t => $"{t.Type.ContainingNamespace}.{t.Type.Name}"))})";

            var hasToString = node.Members
                .Where(m => m.IsKind(SyntaxKind.MethodDeclaration))
                .Cast<MethodDeclarationSyntax>()
                .Any(m => m.Modifiers.Any(mo => mo.IsKind(SyntaxKind.OverrideKeyword)) && m.Identifier.Text == "ToString" && !m.ParameterList.Parameters.Any());

            sourceBuilder.AppendLine(@$"
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace {type.ContainingNamespace}
{{ 
    partial {(node is ClassDeclarationSyntax ? "class" : "struct")} {type.Name} : IEquatable<{type.Name}>, IComparable<{type.Name}>
    {{
        private readonly {innerType} _value; // Do not rename (binary serialization)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {type.Name}({innerType} other)
        {{
            _value = other;
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {type.Name}({string.Join(", ", innerTypes.Select((t, i) => $"{t.Type.ContainingNamespace}.{t.Type.Name} {t.Name.FirstCharToLower()}"))})
        {{
            _value = ({string.Join(", ", innerTypes.Select((t, i) => t.Name.FirstCharToLower()))});
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {type.Name}({(isReadOnly ? "in " : "")}{type.Name} other)
        {{
            _value = other._value;
        }}

");


            for (int i = 0; i < innerTypes.Count(); i++)
            {
                var t = innerTypes.ElementAt(i);
                sourceBuilder.AppendLine(@$"
        public readonly {t.Type.ContainingNamespace}.{t.Type.Name} {t.Name} => _value.Item{i + 1};
");
            }

            sourceBuilder.AppendLine(@$"

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator {innerType}({type.Name} other) => other._value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator {type.Name}({innerType} other) => new {type.Name}(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => _value.Equals(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals({type.Name} obj) => _value.Equals(obj._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo({type.Name} value) => _value.CompareTo(value._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value.CompareTo(right._value) == -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left._value.CompareTo(right._value) == 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right)
        {{
            var result = left._value.CompareTo(right._value);
            return result == -1 || result == 0;
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right)
        {{
            var result = left._value.CompareTo(right._value);
            return result == 1 || result == 0;
        }}
");

            if (!hasToString)
            {

                sourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => _value.ToString();
");
            }

            if (isReadOnly)
            {
                sourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in {type.Name} obj)
        {{
            return _value.Equals(obj._value);
        }}
");
            }

            sourceBuilder.AppendLine(@$"
    }}
}}");

            context.AddSource($"{type.Name}_Implementation.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            return true;
        }

        public void Initialize(InitializationContext context)
        {
        }
    }
}
