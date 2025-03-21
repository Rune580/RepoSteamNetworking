using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RepoSteamNetworking.SourceGenerator.Utils;

namespace RepoSteamNetworking.SourceGenerator;

[Generator]
public class NetworkedPropertyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fieldPipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "RepoSteamNetworking.API.Unity.NetworkedPropertyAttribute",
            predicate: (node, token) =>
            {
                if (node is not BaseFieldDeclarationSyntax fieldDeclSyntax)
                    return false;
                
                return true;
            },
            transform: (syntaxContext, token) =>
            {
                var containingClass = syntaxContext.TargetSymbol.ContainingType;
                // var containingNamespace = containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                
                // var attr = syntaxContext.Attributes[0];
                // var propertyName = attr.NamedArguments.Where(kvp => kvp.Key == "OverridePropertyName")
                //     .Select(kvp => kvp.Value.Value as string)
                //     .FirstOrDefault();

                var propertyName = "";

                // var fieldSymbol = (IFieldSymbol)syntaxContext.TargetSymbol;
                // fieldSymbol = fieldSymbol.;

                // var modifiers = new[]
                // {
                //     syntaxContext.TargetNode.GetType().ToString()
                // };
                
                // var fieldSyntax = (BaseFieldDeclarationSyntax)syntaxContext.TargetNode;
                // var modifiers = fieldSyntax.Modifiers.Select(modifier => modifier.ToFullString())
                //     .ToArray();
                
                var backingField = syntaxContext.TargetSymbol.Name;

                // if (string.IsNullOrEmpty(propertyName))
                //     propertyName = backingField.ToPascalCase();
                
                return new NetworkedPropertyContext
                {
                    PropertyName = "propertyName",
                    BackingFieldName = backingField,
                    Modifiers = [],
                    Namespace = "containingNamespace",
                    ClassName = containingClass.Name,
                };
            }
        );
        
        context.RegisterSourceOutput(fieldPipeline.Collect(), GenerateFromField);
    }

    private void GenerateFromField(SourceProductionContext context, ImmutableArray<NetworkedPropertyContext> propContexts)
    {
        var groupedProps = propContexts.GroupBy(ctx => ctx.FullClassName);

        foreach (var group in groupedProps)
        {
            var props = group.ToArray();

            var code = new CodeBuilder();
            
            code.WithNamespace(props[0].Namespace);

            code.AppendLine($"partial class {props[0].ClassName}\n{{");

            foreach (var prop in props)
            {
                var modifiers = prop.Modifiers.Aggregate("", (current, modifier) => current + $"{modifier} ")
                    .Trim();

                code.AppendLine($"{modifiers} {prop.PropertyName}\n{{")
                    .AppendLine($"get => {prop.BackingFieldName},")
                    .AppendLine("set\n{")
                    .AppendLine($"if ({prop.BackingFieldName} != value)\n{{return;\n}}")
                    .AppendLine($"{prop.BackingFieldName} = value;")
                    .AppendLine("}\n}");
            }

            var sourceText = code.ToSourceText();
            context.AddSource($"{group.Key}_NetworkedFieldProps.g.cs", sourceText);
        }
    }

    private struct NetworkedPropertyContext
    {
        public string PropertyName { get; set; }
        public string BackingFieldName { get; set; }
        public string[] Modifiers { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string FullClassName => $"{Namespace}.{ClassName}";
    }
}