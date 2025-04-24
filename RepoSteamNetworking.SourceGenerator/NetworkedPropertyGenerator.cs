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
                var propertyName = attr.GetNamedArgument<string>("OverridePropertyName");

                var changeKind = attr.GetNamedArgument<byte>("SendMethod");
                
                var writePerms = attr.GetNamedArgument<byte>("WritePermissions");
                
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
                    ChangeKind = changeKind,
                    WritePermissions = writePerms
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
                
                var attr = syntaxContext.Attributes[0];
                var changeKind = attr.GetNamedArgument<byte>("SendMethod");
                
                var writePerms = attr.GetNamedArgument<byte>("WritePermissions");
                
                var overrideBackingField = attr.GetNamedArgument<string>("OverrideBackingField");
                
                var callbackMethodName = attr.GetNamedArgument<string>("CallbackMethodName");
                
                var propertySymbol = (IPropertySymbol)syntaxContext.TargetSymbol;
                var propertySyntax = (BasePropertyDeclarationSyntax)syntaxContext.TargetNode;
                
                var modifiers = propertySyntax!.Modifiers.Select(modifier => modifier.ToFullString().Trim())
                    .ToArray();
                
                var type = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                var propertyName = syntaxContext.TargetSymbol.Name.Trim();

                var backingField = $"{propertyName}_BackingField".ToLowerCamelCaseWithUnderscore();
                var needsField = true;
                
                if (!string.IsNullOrWhiteSpace(overrideBackingField))
                {
                    backingField = overrideBackingField;
                    needsField = false;
                }

                return new NetworkedPropertyContext
                {
                    PropertyName = propertyName,
                    BackingFieldName = backingField,
                    Modifiers = modifiers,
                    TypeName = type,
                    Namespace = containingNamespace,
                    ClassName = containingClass.Name,
                    NeedsField = needsField,
                    BaseTypeTree = baseTypeTree,
                    ChangeKind = changeKind,
                    WritePermissions = writePerms,
                    CallbackMethodName = callbackMethodName
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
            
            code.WithImports("RepoSteamNetworking.API", "RepoSteamNetworking.API.Unity", "RepoSteamNetworking.Networking.NetworkedProperties", "RepoSteamNetworking.Networking.Unity", "RepoSteamNetworking.Networking.Attributes")
                .WithNamespace(props[0].Namespace);

            code.AppendLine($"[GenerateBehaviourId]\npartial class {props[0].ClassName} : INetworkedPropertyListener\n{{");
            
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var modifiers = prop.Modifiers.Aggregate("", (current, modifier) => current + $"{modifier} ")
                    .Trim();

                if (prop.NeedsField)
                    code.AppendLine($"private {prop.TypeName} {prop.BackingFieldName};");

                code.AppendLine($"{modifiers} {prop.TypeName} {prop.PropertyName}\n{{")
                    .AppendLine($"get => {prop.BackingFieldName};")
                    .AppendLine("set\n{");

                switch (prop.WritePermissions)
                {
                    case 0: // Host
                        code.AppendLine("if (!RepoSteamNetwork.IsServer)\n{\nreturn;\n}");
                        break;
                    case 1: // Everyone
                        break;
                    case 2: // Owner
                        // TODO
                        break;
                }
                
                code.AppendLine($"if ({prop.BackingFieldName} == value)\n{{\nreturn;\n}}");

                switch (prop.ChangeKind)
                {
                    case 0:
                        code.AppendLine($"{prop.BackingFieldName} = value;")
                            .AppendLine($"SendNetworkedPropertyData({prop.BackingFieldName}, {i}, {prop.ChangeKind});");
                        break;
                    case 1:
                        code.AppendLine($"SendNetworkedPropertyData(value - {prop.BackingFieldName}, {i}, {prop.ChangeKind});")
                            .AppendLine($"{prop.BackingFieldName} = value;");
                        break;
                }
                
                code.AppendLine("}\n}");
            }

            code.AppendLine("private void SendNetworkedPropertyData(object value, uint propId, byte changeKind)\n{")
                .AppendLine("var networkIdentity = GetNetworkIdentity();")
                .AppendLine($"NetworkedPropertyManager.AddNetworkedPropertyDataToQueue(value, networkIdentity.NetworkId, ModGuid, SubId, \"{group.Key}\", propId, changeKind);")
                .AppendLine("}");

            if (props[0].HasNetworkPropertyListener)
            {
                code.AppendLine("public override void OnNetworkedPropertiesDataReceived(string targetClass, NetworkedPropertyChange[] props)\n{")
                    .AppendLine("base.OnNetworkedPropertiesDataReceived(targetClass, props);");
            }
            else
            {
                code.AppendLine("public virtual void OnNetworkedPropertiesDataReceived(string targetClass, NetworkedPropertyChange[] props)\n{");
            }
            
            code.AppendLine($"if (targetClass != \"{group.Key}\")\n{{")
                .AppendLine("return;\n}")
                .AppendLine("foreach (NetworkedPropertyChange prop in props)\n{")
                .AppendLine("SetNetworkedPropertyValue(prop.PropertyId, prop.Value);\n}")
                .AppendLine("}");

            code.AppendLine("private void SetNetworkedPropertyValue(uint propId, object value)\n{")
                .AppendLine("switch (propId)\n{");
            
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                
                code.AppendLine($"case {i}:");

                switch (prop.ChangeKind)
                {
                    case 0: // Set
                        code.AppendLine($"{prop.BackingFieldName} = ({prop.TypeName})value;");
                        break;
                    case 1: // Deltas
                        code.AppendLine($"{prop.BackingFieldName} += ({prop.TypeName})value;");
                        break;
                }
                
                
                if (!string.IsNullOrWhiteSpace(prop.CallbackMethodName))
                    code.AppendLine($"{prop.CallbackMethodName}(({prop.TypeName})value);");
                
                code.AppendLine("break;");
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
        public string CallbackMethodName { get; set; }
        public string BaseTypeTree { get; set; }
        public bool HasNetworkPropertyListener { get; set; }
        public byte ChangeKind { get; set; }
        public byte WritePermissions { get; set; }
        public string FullClassName => $"{Namespace}.{ClassName}";
    }
}