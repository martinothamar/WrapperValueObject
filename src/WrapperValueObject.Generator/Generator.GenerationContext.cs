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
            public readonly IEnumerable<(string Name, INamedTypeSymbol Type)> InnerTypes;

            public GenerationContext(
                GeneratorExecutionContext context, 
                StringBuilder sourceBuilder, 
                TypeDeclarationSyntax node, 
                ISymbol type, 
                IEnumerable<(string Name, INamedTypeSymbol Type)> innerTypes
            )
            {
                Context = context;
                SourceBuilder = sourceBuilder;
                Node = node;
                Type = type;
                InnerTypes = innerTypes;
            }
        }
    }
}
