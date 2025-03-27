using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RepoSteamNetworking.SourceGenerator.Utils;

namespace RepoSteamNetworking.SourceGenerator;

[Generator]
public class RPCMethodGenerator : IIncrementalGenerator
{
    public const string AttributeClassName = "RepoSteamNetworking.API.Unity.RepoSteamRPCAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: AttributeClassName,
            predicate: RPCMethodPredicate,
            transform: (syntaxContext, token) =>
            {
                var rpcTarget = syntaxContext.Attributes[0].ConstructorArguments[0];
                
                var containingClass = syntaxContext.TargetSymbol.ContainingType;
                
                var methodName = syntaxContext.TargetSymbol.Name;

                var methodSymbol = (IMethodSymbol)syntaxContext.TargetSymbol;
                var parameterSymbols = methodSymbol.Parameters.ToArray();

                return new RpcMethodContext(
                    containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                    containingClass.Name,
                    methodName,
                    parameterSymbols,
                    rpcTarget
                );
            }
        );
        
        context.RegisterSourceOutput(pipeline.Collect(), GenerateOutput);
    }

    public static bool RPCMethodPredicate(SyntaxNode node, CancellationToken token)
    {
        if (node is not MethodDeclarationSyntax methodDeclarationSyntax)
            return false;

        return methodDeclarationSyntax.Identifier.Text.EndsWith("RPC");
    }

    private static void GenerateOutput(SourceProductionContext context, ImmutableArray<RpcMethodContext> methods)
    {
        if (methods.Length == 0)
            return;

        var groupedMethods = methods.GroupBy(methodContext => methodContext.FullClassName);

        foreach (var methodGroup in groupedMethods)
        {
            var classMethods = methodGroup.ToArray();
            
            var code = new CodeBuilder();

            code.WithImports("RepoSteamNetworking.API", "RepoSteamNetworking.API.Unity")
                .WithNamespace(classMethods[0].Namespace); 
            
            code.AppendLine($"partial class {classMethods[0].ClassName}\n{{");
             
            foreach (var rpcMethodContext in classMethods)
            {
                code.AppendLine(GenerateRPCMethod(rpcMethodContext));
            }

            var sourceText = code.ToSourceText();
            context.AddSource($"{methodGroup.Key}_RPCMethods.g.cs", sourceText);
        }
    }

    private static string GenerateRPCMethod(RpcMethodContext rpcMethodContext)
    {
        var methodParameters = "";
        var paramNames = "";
        foreach (var parameterSymbol in rpcMethodContext.Parameters)
        {
            var paramWithType = parameterSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                    .WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeModifiers | SymbolDisplayParameterOptions.IncludeDefaultValue)
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            );

            methodParameters += $"{paramWithType}, ";

            var paramName = parameterSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            paramNames += $"{paramName}, ";
        }
        
        if (!string.IsNullOrEmpty(methodParameters) && !string.IsNullOrEmpty(paramNames))
        {
            methodParameters = methodParameters.Substring(0, methodParameters.Length - 2);
            paramNames = $", {paramNames.Substring(0, paramNames.Length - 2)}";
        }

        var generatedMethodName = rpcMethodContext.MethodName.Substring(0, rpcMethodContext.MethodName.Length - 3);

        var methodCode = $$"""
                           public void {{generatedMethodName}}({{methodParameters}})
                           {
                               var networkIdentity = GetNetworkIdentity();
                               
                               if (!networkIdentity || !networkIdentity.IsValid)
                               {
                                   UnityEngine.Debug.LogError($"Failed to call rpc {{rpcMethodContext.MethodName}} as there was no valid network identity!\nMake sure there's a {{SteamNetworkSubIdentityImplementerGenerator.NetworkIdentityClassName}} component on the root of the prefab that the component {{rpcMethodContext.FullClassName}} is attached to!");
                                   return;
                               }
                               
                               var networkId = networkIdentity.NetworkId;
                               RepoSteamNetwork.CallRPC({{rpcMethodContext.RPCTarget.Value}}, networkId, ModGuid, SubId, nameof({{rpcMethodContext.MethodName}}){{paramNames}});
                           }
                           """;
        
        return methodCode;
    }

    private struct RpcMethodContext(string @namespace, string className, string methodName, IParameterSymbol[] parameters, TypedConstant rpcTarget)
    {
        public string Namespace = @namespace;
        public string ClassName = className;
        public string MethodName = methodName;
        public IParameterSymbol[] Parameters = parameters;
        public TypedConstant RPCTarget = rpcTarget;
        
        public string FullClassName => $"{Namespace}.{ClassName}";
    }
}