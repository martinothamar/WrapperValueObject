using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

namespace WrapperValueObject.Generator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        private const string AttributeTypeName = "WrapperValueObject.WrapperValueObjectAttribute";
        private const string AssemblyName = "WrapperValueObject";

        public void Execute(SourceGeneratorContext context)
        {
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

                    if (!node.Modifiers.Any())
                        continue;

                    if (!node.AttributeLists.Any())
                        continue;

                    var type = semanticModel.GetDeclaredSymbol(node);
                    var attribute = type!.GetAttributes().SingleOrDefault(a => a.AttributeClass?.Name == "WrapperValueObjectAttribute");
                    if (attribute is null)
                        continue;

                    var wrapperType = ((TypedConstant)attribute.ConstructorArguments[0]).Value as INamedTypeSymbol;

                    var csharpType = $"{wrapperType!.ContainingNamespace}.{wrapperType.Name}" switch
                    {
                        "System.Single" => GenerateFloat(sourceBuilder, node, type),
                        _ => throw new Exception("Invalid"),
                    };                    

                    //System.Diagnostics.Debugger.Launch();
                }
            }


            context.AddSource("Wrappers.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private bool GenerateFloat(StringBuilder sourceBuilder, TypeDeclarationSyntax node, ISymbol type)
        {
            var isReadOnly = node.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

            // TODO - options for methods to generate, don't generate methods where the user has already supplied them (i.e. custom ToString)

            sourceBuilder.AppendLine(@$"
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace {type.ContainingNamespace}
{{ 
    partial {(node is ClassDeclarationSyntax ? "class" : "struct")} {type.Name} : IEquatable<{type.Name}>, IComparable<{type.Name}>, IComparable
    {{
        private readonly float m_value; // Do not rename (binary serialization)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {type.Name}(float other)
        {{
            m_value = other;
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public {type.Name}({(isReadOnly ? "in " : "")}{type.Name} other)
        {{
            m_value = other.m_value;
        }}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float({type.Name} other) => other.m_value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator {type.Name}(float other) => new {type.Name}(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => m_value.ToString();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(IFormatProvider? provider) => m_value.ToString(provider);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string? format) => m_value.ToString(format);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string? format, IFormatProvider? provider) => m_value.ToString(format, provider);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
            => m_value.TryFormat(destination, out charsWritten, format, provider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => m_value.Equals(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals({type.Name} obj) => m_value.Equals(obj.m_value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object? value) => m_value.CompareTo(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo({type.Name} value) => m_value.CompareTo(value.m_value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left.m_value == right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left.m_value != right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left.m_value < right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left.m_value > right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left.m_value <= right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => left.m_value >= right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator +({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left.m_value + right.m_value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator -({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left.m_value - right.m_value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator /({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left.m_value / right.m_value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator *({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left.m_value * right.m_value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static {type.Name} operator %({(isReadOnly ? "in " : "")}{type.Name} left, {(isReadOnly ? "in " : "")}{type.Name} right) => new {type.Name}(left.m_value % right.m_value);
");

            if (isReadOnly)
            {
                sourceBuilder.AppendLine(@$"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in {type.Name} obj)
        {{
            return m_value.Equals(obj.m_value);
        }}
");
            }

            sourceBuilder.AppendLine(@$"
    }}
}}");

            return true;
        }

        public void Initialize(InitializationContext context)
        {
        }
    }
}
