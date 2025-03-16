using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RepoSteamNetworking.SourceGenerator;

[Generator]
public class RPCIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialization);

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "RepoSteamNetworking.API.RepoSteamRPCAttribute",
            predicate: (node, token) =>
            {
                if (node is not MethodDeclarationSyntax methodDeclarationSyntax)
                    return false;

                return methodDeclarationSyntax.Identifier.Text.EndsWith("RPC");
            },
            transform: (syntaxContext, token) =>
            {
                var containingClass = syntaxContext.TargetSymbol.ContainingType;

                var methodName = syntaxContext.TargetSymbol.Name;
                methodName = methodName.Substring(0, methodName.Length - 3);

                return new RpcMethod(
                    containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                    containingClass.Name,
                    methodName
                );
            }
        );
        
        context.RegisterSourceOutput(pipeline, GenerateOutput);
    }

    private void PostInitialization(IncrementalGeneratorPostInitializationContext ctx)
    {
        
    }

    private void GenerateOutput(SourceProductionContext context, RpcMethod rpcMethod)
    {
        var sourceText = SourceText.From($$"""
                                           using RepoSteamNetworking.API;
                                           using UnityEngine;
                                           namespace {{rpcMethod.Namespace}};
                                           partial class {{rpcMethod.ClassName}}
                                           {
                                                public void {{rpcMethod.MethodName}}()
                                                {
                                                    Debug.Log("Hello from {{rpcMethod.MethodName}}!");
                                                }
                                           }
                                           """, Encoding.UTF8);
        
        context.AddSource($"{rpcMethod.ClassName}_{rpcMethod.MethodName}.g.cs", sourceText);
    }

    private struct RpcMethod(string @namespace, string className, string methodName)
    {
        public string Namespace = @namespace;
        public string ClassName = className;
        public string MethodName = methodName;
    }
}