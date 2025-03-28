using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RepoSteamNetworking.SourceGenerator.Utils;

namespace RepoSteamNetworking.SourceGenerator;

[Generator]
public class SteamNetworkSubIdentityImplementerGenerator : IIncrementalGenerator
{
    public const string NetworkIdentityClassName = "RepoSteamNetworkIdentity";
    private const string GeneratedInfoClassName = "RepoSteamNetwork_Generated_PluginInfo";
    private const string PluginGuidFieldName = "PLUGIN_GUID";
    
    private static readonly Regex PluginInfoRegex = new(@"obj/(?:Release|Debug)/.+?/(?:MyPluginInfo|PluginInfo)\.cs$", RegexOptions.Compiled);
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Search for a generated PluginInfo.cs or MyPluginInfo.cs from BepInEx.PluginInfoProps for the assembly
        var pluginInfoModGuid = context.CompilationProvider.Select(static (compilation, token) =>
        {
            var comp = (CSharpCompilation)compilation;

            var propsModGuidGetter = "";
            var @namespace = "";
            
            foreach (var location in comp.Assembly.Locations)
            {
                if (location.Kind is not LocationKind.SourceFile || location.SourceTree is null)
                    continue;

                if (!PluginInfoRegex.IsMatch(location.SourceTree.FilePath))
                    continue;
                
                if (!location.SourceTree.TryGetRoot(out var root))
                    continue;
                
                foreach (var child in root.DescendantNodes())
                {
                    if (child is NamespaceDeclarationSyntax namespaceDecl)
                    {
                        @namespace = namespaceDecl.Name.ToString();
                        continue;
                    }
                    
                    if (child is not FieldDeclarationSyntax field)
                        continue;

                    var variable = field.Declaration.Variables.First();

                    var identifier = variable.Identifier.Text;
                    if (identifier != "PLUGIN_GUID")
                        continue;

                    if (variable.Initializer?.Value is not LiteralExpressionSyntax literal)
                        continue;

                    if (!literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                        continue;
                    
                    var className = location.SourceTree.FilePath.EndsWith("MyPluginInfo.cs") ? "MyPluginInfo" : "PluginInfo";
                    
                    propsModGuidGetter = $"{@namespace}.{className}.{PluginGuidFieldName}";
                    break;
                }
            }
            
            return new ModGuidGetterInfo
            {
                ModGuidGetter = propsModGuidGetter,
                Namespace = @namespace,
            };
        });

        var bepInPluginAttributePipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "BepInEx.BepInPlugin",
            predicate: static (node, token) =>
            {
                return node is ClassDeclarationSyntax;
            },
            transform: static (syntaxContext, token) =>
            {
                var containingClass = syntaxContext.TargetSymbol;
                var containingNamespace = containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                if (containingNamespace is null)
                    return new BepInPluginGuid();
                
                var bepInPluginAttr = syntaxContext.Attributes[0];

                var guids = bepInPluginAttr.NamedArguments.Where(kvp => kvp.Key == "GUID")
                    .Select(kvp => kvp.Value)
                    .ToArray();

                var guidTypedConstant = guids.Length == 1 ? guids.Single() : bepInPluginAttr.ConstructorArguments[0];

                var modGuid = "";
                if (guidTypedConstant.Value is string guid && !string.IsNullOrEmpty(guid))
                    modGuid = guid;
                
                return new BepInPluginGuid
                {
                    ModGuid = modGuid,
                    Namespace = containingNamespace,
                };
            }
        );

        var modGuidGetters = pluginInfoModGuid
            .Combine(bepInPluginAttributePipeline.Collect())
            .Select(static (pair, token) =>
            {
                var getters = new HashSet<ModGuidGetterInfo>();

                if (pair.Right.Length <= 0)
                {
                    getters.Add(pair.Left);
                    return [..getters];
                }

                foreach (var bepInPlugin in pair.Right)
                {
                    // Do we already have a Guid getter from (My)PluginInfo.cs? Skip if we do.
                    if (pair.Left.Namespace.Contains(bepInPlugin.Namespace) && !string.IsNullOrEmpty(pair.Left.ModGuidGetter))
                    {
                        getters.Add(pair.Left);
                        continue;
                    }

                    if (string.IsNullOrEmpty(bepInPlugin.ModGuid))
                        continue;

                    var getter = new ModGuidGetterInfo
                    {
                        ModGuidGetter = $"{bepInPlugin.Namespace}.{GeneratedInfoClassName}.{PluginGuidFieldName}",
                        Namespace = bepInPlugin.Namespace,
                        NeedsGeneration = true,
                        ModGuid = bepInPlugin.ModGuid,
                    };

                    getters.Add(getter);
                }

                return getters.ToImmutableArray();
            });

        var gettersToGenerate = modGuidGetters.Select(static (getterInfos, token) =>
            getterInfos.Where(info => info.NeedsGeneration).ToImmutableArray());
        
        context.RegisterSourceOutput(gettersToGenerate, GenerateGetters);
        
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

        implementers = implementers.Combine(modGuidGetters)
            .Select(static (pair, token) =>
            {
                var result = new HashSet<ImplementerContext>();
                
                for (var i = 0; i < pair.Left.Length; i++)
                {
                    var impl = pair.Left[i];
                    
                    if (impl.ModGuidSource is ModGuidSourceKind.UserDefined)
                    {
                        result.Add(impl);
                        continue;
                    }

                    var getter = "";
                    foreach (var info in pair.Right)
                    {
                        if (string.IsNullOrEmpty(info.ModGuidGetter) || string.IsNullOrEmpty(info.Namespace))
                            continue;
                        
                        if (impl.Namespace.Contains(info.Namespace))
                        {
                            getter = info.ModGuidGetter;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(getter))
                    {
                        result.Add(impl);
                        continue;
                    }

                    impl.ModGuidSource = ModGuidSourceKind.BepInExPluginInfoPropsGetter;
                    impl.ModGuid = getter;
                    
                    result.Add(impl);
                }

                return result.ToImmutableArray();
            });
        
        context.RegisterSourceOutput(implementers, GenerateImplementers);
    }

    private static void GenerateGetters(SourceProductionContext context, ImmutableArray<ModGuidGetterInfo> getterInfos)
    {
        foreach (var info in getterInfos)
        {
            if (!info.NeedsGeneration || string.IsNullOrEmpty(info.ModGuid))
                continue;
            
            var fullClassName = $"{info.Namespace}.{GeneratedInfoClassName}";

            var code = new CodeBuilder();

            code.WithNamespace(info.Namespace);

            code.AppendLine($"internal static class {GeneratedInfoClassName}\n{{");
            code.AppendLine($"public const string {PluginGuidFieldName} = \"{info.ModGuid}\";");
            
            context.AddSource($"{fullClassName}.g.cs", code.ToSourceText());
        }
    }

    private static ImplementerContext TransformImplementer(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token)
    {
        var containingClass = syntaxContext.TargetSymbol.ContainingType;
        var containingNamespace = containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        
        var possibleModGuidImplementations = containingClass.GetMembers("ModGuid");
        var hasUserDefinedGuid = possibleModGuidImplementations.Any(symbol => symbol.Kind == SymbolKind.Property);

        var modGuidSource = ModGuidSourceKind.Unknown;
        if (hasUserDefinedGuid)
            modGuidSource = ModGuidSourceKind.UserDefined;

        return new ImplementerContext
        {
            Namespace = containingNamespace,
            ClassName = containingClass.Name,
            ModGuidSource = modGuidSource,
        };
    }

    private static void GenerateImplementers(SourceProductionContext context, ImmutableArray<ImplementerContext> implementers)
    {
        foreach (var implementer in implementers)
        {
            var code = new CodeBuilder();
            
            code.WithImports("RepoSteamNetworking.API.Unity")
                .WithNamespace(implementer.Namespace);
            
            code.AppendLine($"partial class {implementer.ClassName} : ISteamNetworkSubIdentity\n{{");

            switch (implementer.ModGuidSource)
            {
                case ModGuidSourceKind.Unknown:
                case ModGuidSourceKind.UserDefined:
                    break;
                case ModGuidSourceKind.BepInExPluginInfoPropsGetter:
                    code.AppendLine($"public string ModGuid => {implementer.ModGuid};");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
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

    private record struct ImplementerContext
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public ModGuidSourceKind ModGuidSource { get; set; }
        public string ModGuid { get; set; }
        
        public string FullClassName => $"{Namespace}.{ClassName}";
    }

    private record struct ModGuidGetterInfo
    {
        public string ModGuidGetter { get; set; }
        public string Namespace { get; set; }
        public bool NeedsGeneration { get; set; }
        public string ModGuid { get; set; }
    }

    private record struct BepInPluginGuid
    {
        public string ModGuid { get; set; }
        public string Namespace { get; set; }
    }

    private enum ModGuidSourceKind
    {
        Unknown,
        UserDefined,
        BepInExPluginInfoPropsGetter,
    }
}