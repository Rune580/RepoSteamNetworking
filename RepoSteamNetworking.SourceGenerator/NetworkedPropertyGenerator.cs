using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RepoSteamNetworking.SourceGenerator.Utils;

namespace RepoSteamNetworking.SourceGenerator;

[Generator]
public class NetworkedPropertyGenerator : IIncrementalGenerator
{
    public const string AttributeClassName = "RepoSteamNetworking.API.Unity.NetworkedPropertyAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fieldPipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: AttributeClassName,
            predicate: static (node, token) =>
            {
                if (node is VariableDeclaratorSyntax variableDeclaratorSyntax)
                {
                    if (variableDeclaratorSyntax.Parent is VariableDeclarationSyntax { Parent: not BaseFieldDeclarationSyntax })
                        return false;
                }
                else if (node is not BaseFieldDeclarationSyntax fieldDeclSyntax)
                {
                    return false;
                }

                return true;
            },
            transform: static (syntaxContext, token) =>
            {
                var containingClass = syntaxContext.TargetSymbol.ContainingType;
                var containingNamespace = containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                var baseType = containingClass.BaseType;
                var baseTypeTree = containingClass.Name;
                while (baseType is not null)
                {
                    baseTypeTree = $"{baseType.Name}.{baseTypeTree}";
                    baseType = baseType.BaseType;
                }
                
                var attr = syntaxContext.Attributes[0];
                var propertyName = attr.NamedArguments.Where(kvp => kvp.Key == "OverridePropertyName")
                    .Select(kvp => kvp.Value.Value as string)
                    .FirstOrDefault();
                
                var fieldSymbol = (IFieldSymbol)syntaxContext.TargetSymbol;

                FieldDeclarationSyntax fieldSyntax = null;

                if (syntaxContext.TargetNode is VariableDeclaratorSyntax declaratorSyntax)
                {
                    if (declaratorSyntax.Parent is VariableDeclarationSyntax { Parent: FieldDeclarationSyntax fieldDeclarationSyntax })
                        fieldSyntax = fieldDeclarationSyntax;
                }
                else
                {
                    fieldSyntax = (FieldDeclarationSyntax)syntaxContext.TargetNode;
                }
                
                var modifiers = fieldSyntax!.Modifiers.Select(modifier => modifier.ToFullString().Trim())
                    .ToArray();

                var type = fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                
                var backingField = syntaxContext.TargetSymbol.Name;

                if (string.IsNullOrEmpty(propertyName))
                    propertyName = backingField.ToPascalCase();
                
                return new NetworkedPropertyContext
                {
                    PropertyName = propertyName,
                    BackingFieldName = backingField,
                    Modifiers = modifiers,
                    TypeName = type,
                    Namespace = containingNamespace,
                    ClassName = containingClass.Name,
                    BaseTypeTree = baseTypeTree,
                };
            }
        );

        var propertyPipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: AttributeClassName,
            predicate: static (node, token) =>
            {
                if (node is not BasePropertyDeclarationSyntax propertyDeclarationSyntax)
                    return false;

                return true;
            },
            transform: static (syntaxContext, token) =>
            {
                var containingClass = syntaxContext.TargetSymbol.ContainingType;
                var containingNamespace = containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                var baseType = containingClass.BaseType;
                var baseTypeTree = containingClass.Name;
                while (baseType is not null)
                {
                    baseTypeTree = $"{baseType.Name}.{baseTypeTree}";
                    baseType = baseType.BaseType;
                }
                
                var propertySymbol = (IPropertySymbol)syntaxContext.TargetSymbol;
                var propertySyntax = (BasePropertyDeclarationSyntax)syntaxContext.TargetNode;
                
                var modifiers = propertySyntax!.Modifiers.Select(modifier => modifier.ToFullString().Trim())
                    .ToArray();
                
                var type = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                var propertyName = syntaxContext.TargetSymbol.Name.Trim();

                var backingField = $"{propertyName}_BackingField".ToLowerCamelCaseWithUnderscore();
                
                return new NetworkedPropertyContext
                {
                    PropertyName = propertyName,
                    BackingFieldName = backingField,
                    Modifiers = modifiers,
                    TypeName = type,
                    Namespace = containingNamespace,
                    ClassName = containingClass.Name,
                    NeedsField = true,
                    BaseTypeTree = baseTypeTree,
                };
            }
        );

        var networkProps = fieldPipeline.Collect()
            .Combine(propertyPipeline.Collect())
            .Select(static (pair, token) =>
            {
                var props = new NetworkedPropertyContext[pair.Left.Length + pair.Right.Length];
                var i = 0;

                foreach (var prop in pair.Left)
                    props[i++] = prop;

                foreach (var prop in pair.Right)
                    props[i++] = prop;
                
                // Hacky solution to figure out if a class containing a networked property inherits another class that also contains a networked property.
                // Since we need to implement an interface on the base class and allow inheriting classes to override the listener.
                // Need to see if there's a better solution for this.
                for (var j = 0; j < props.Length; j++)
                {
                    var prop = props[j];

                    foreach (var subProp in props)
                    {
                        if (prop.FullClassName == subProp.FullClassName)
                            continue;
                        
                        if (!prop.BaseTypeTree.Contains(subProp.BaseTypeTree))
                            continue;

                        prop.HasNetworkPropertyListener = true;
                        break;
                    }
                    
                    props[j] = prop;
                }

                return props.ToImmutableArray();
            });
        
        context.RegisterSourceOutput(networkProps, GenerateNetworkProperty);
    }

    private static void GenerateNetworkProperty(SourceProductionContext context, ImmutableArray<NetworkedPropertyContext> propContexts)
    {
        var groupedProps = propContexts.GroupBy(ctx => ctx.FullClassName);

        foreach (var group in groupedProps)
        {
            var props = group.ToArray();

            var code = new CodeBuilder();
            
            code.WithImports("RepoSteamNetworking.API.Unity", "RepoSteamNetworking.Networking.NetworkedProperties")
                .WithNamespace(props[0].Namespace);

            code.AppendLine($"partial class {props[0].ClassName} : INetworkedPropertyListener\n{{");
            
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var modifiers = prop.Modifiers.Aggregate("", (current, modifier) => current + $"{modifier} ")
                    .Trim();

                if (prop.NeedsField)
                    code.AppendLine($"private {prop.TypeName} {prop.BackingFieldName};");

                code.AppendLine($"{modifiers} {prop.TypeName} {prop.PropertyName}\n{{")
                    .AppendLine($"get => {prop.BackingFieldName};")
                    .AppendLine("set\n{")
                    .AppendLine($"if ({prop.BackingFieldName} == value)\n{{\nreturn;\n}}")
                    .AppendLine($"{prop.BackingFieldName} = value;")
                    .AppendLine($"SendNetworkedPropertyData({prop.BackingFieldName}, {i});")
                    .AppendLine("}\n}");
            }

            code.AppendLine("private void SendNetworkedPropertyData(object value, uint propId)\n{")
                .AppendLine("var networkIdentity = GetNetworkIdentity();")
                .AppendLine($"NetworkedPropertyManager.AddNetworkedPropertyDataToQueue(value, networkIdentity.NetworkId, ModGuid, SubId, \"{group.Key}\", propId);")
                .AppendLine("}");

            if (props[0].HasNetworkPropertyListener)
            {
                code.AppendLine("public override void OnNetworkedPropertyReceived()\n{")
                    .AppendLine("base.OnNetworkedPropertyReceived();");
            }
            else
            {
                code.AppendLine("public virtual void OnNetworkedPropertyReceived()\n{");
            }

            // TODO implement receiving and setting property data.
            code.AppendLine("")
                .AppendLine("}");

            code.AppendLine("private void SetNetworkedPropertyValue(uint propId, object value)\n{")
                .AppendLine("switch (propId)\n{");
            
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                
                code.AppendLine($"case {i}:")
                    .AppendLine($"{prop.BackingFieldName} = ({prop.TypeName})value;")
                    .AppendLine("break;");
            }
            code.AppendLine("default:")
                .AppendLine("break;")
                .AppendLine("}\n}");

            var sourceText = code.ToSourceText();
            context.AddSource($"{group.Key}_NetworkedProperties.g.cs", sourceText);
        }
    }

    private struct NetworkedPropertyContext
    {
        public string PropertyName { get; set; }
        public string BackingFieldName { get; set; }
        public string[] Modifiers { get; set; }
        public string TypeName { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public bool NeedsField { get; set; }
        public string BaseTypeTree { get; set; }
        public bool HasNetworkPropertyListener { get; set; }
        public string FullClassName => $"{Namespace}.{ClassName}";
    }
}