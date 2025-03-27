using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RepoSteamNetworking.SourceGenerator.Utils;

namespace RepoSteamNetworking.SourceGenerator;

[Generator]
public class SteamNetworkSubIdentityImplementerGenerator : IIncrementalGenerator
{
    public const string NetworkIdentityClassName = "RepoSteamNetworkIdentity";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rpcAttributePipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: RPCMethodGenerator.AttributeClassName,
            predicate: RPCMethodGenerator.RPCMethodPredicate,
            transform: TransformImplementer
        );

        var networkedPropertyAttributePipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: NetworkedPropertyGenerator.AttributeClassName,
            predicate: static (node, token) =>
            {
                if (node is not VariableDeclaratorSyntax and BasePropertyDeclarationSyntax)
                    return false;

                return true;
            },
            transform: TransformImplementer
        );

        var implementers = rpcAttributePipeline.Collect()
            .Combine(networkedPropertyAttributePipeline.Collect())
            .Select(static (pair, token) =>
            {
                var result = new HashSet<ImplementerContext>();

                foreach (var ctx in pair.Left)
                    result.Add(ctx);

                foreach (var ctx in pair.Right)
                    result.Add(ctx);
                
                return result.ToImmutableArray();
            });
        
        context.RegisterSourceOutput(implementers, GenerateImplementers);
    }

    private static void GenerateImplementers(SourceProductionContext context, ImmutableArray<ImplementerContext> implementers)
    {
        foreach (var implementer in implementers)
        {
            var code = new CodeBuilder();
            
            code.WithImports("RepoSteamNetworking.API.Unity")
                .WithNamespace(implementer.Namespace);
            
            code.AppendLine($"partial class {implementer.ClassName} : ISteamNetworkSubIdentity\n{{");
            
            code.AppendLine($$"""
                              public uint SubId { get; set; }

                              public bool IsValid { get; set; }

                              private {{NetworkIdentityClassName}} _networkIdentity;

                              public {{NetworkIdentityClassName}} GetNetworkIdentity()
                              {
                                  if (!_networkIdentity)
                                  {
                                      _networkIdentity = GetComponentInParent<{{NetworkIdentityClassName}}>(true);
                                  }
                                  
                                  return _networkIdentity;
                              }
                              """);

            context.AddSource($"{implementer.FullClassName}_SubIdentityImpl.g.cs", code.ToSourceText());
        }
    }

    private static ImplementerContext TransformImplementer(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token)
    {
        var containingClass = syntaxContext.TargetSymbol.ContainingType;
        var containingNamespace = containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

        return new ImplementerContext { Namespace = containingNamespace, ClassName = containingClass.Name, };
    }

    private record struct ImplementerContext
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        
        public string FullClassName => $"{Namespace}.{ClassName}";
    }
}