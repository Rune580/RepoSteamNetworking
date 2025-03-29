using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace RepoSteamNetworking.SourceGenerator.Tests;

[TestFixture]
public class NetworkedPropertyTests
{
    private static string[] GetRequiredCode()
    {
        var networkAttrFile = File.ReadAllText($"../../../../RepoSteamNetworking/API/Unity/NetworkedPropertyAttribute.cs");
        var listenerInterfaceFile = File.ReadAllText($"../../../../RepoSteamNetworking/API/Unity/INetworkedPropertyListener.cs");
        
        return [networkAttrFile, listenerInterfaceFile];
    }
    
    [Test]
    public async Task FromFieldTest()
    {
        var code = """
                   using RepoSteamNetworking.API.Unity;
                   
                   namespace ExampleNamespace
                   {
                        public partial class ExampleBehaviour
                        {
                            [NetworkedProperty]
                            public int testNumber;
                        }
                   }
                   """;

        var expectedFileName = "RepoSteamNetworking.SourceGenerator/RepoSteamNetworking.SourceGenerator.NetworkedPropertyGenerator/ExampleNamespace.ExampleBehaviour_NetworkedFieldProps.g.cs";
        var expectedCode = """
                           namespace ExampleNamespace
                           {
                                partial class ExampleBehaviour
                                {
                                    public int TestNumber
                                    {
                                        get => testNumber;
                                        set
                                        {
                                            if (testNumber == value)
                                            {
                                                return;
                                            }
                                            testNumber = value;
                                        }
                                    }
                                }
                           }
                           """;

        expectedCode = CSharpSyntaxTree.ParseText(expectedCode)
            .GetRoot()
            .NormalizeWhitespace()
            .ToFullString();
        
        var requiredCode = GetRequiredCode();
        
        var runner = new CSharpSourceGeneratorVerifier<NetworkedPropertyGenerator>.TestRunner
        {
            TestState =
            {
                Sources = { ("ExampleBehaviour.cs", code), requiredCode[0], requiredCode[1] },
                GeneratedSources = { (expectedFileName, SourceText.From(expectedCode, Encoding.UTF8)) },
            }
        };

        await runner.RunAsync();
    }

    [Test]
    public async Task FromAutoPropertyTest()
    {
        var code = """
                   using RepoSteamNetworking.API.Unity;

                   namespace ExampleNamespace
                   {
                        public partial class ExampleBehaviour
                        {
                            [NetworkedProperty]
                            public partial int TestNumber { get; set; }
                        }
                   }
                   """;
        
        var expectedFileName = "RepoSteamNetworking.SourceGenerator/RepoSteamNetworking.SourceGenerator.NetworkedPropertyGenerator/ExampleNamespace.ExampleBehaviour_NetworkedAutoProps.g.cs";
        var expectedCode = """
                           namespace ExampleNamespace
                           {
                                partial class ExampleBehaviour
                                {
                                    public partial int TestNumber
                                    {
                                        get => field;
                                        set
                                        {
                                            if (field == value)
                                            {
                                                return;
                                            }
                                            field = value;
                                        }
                                    }
                                }
                           }
                           """;
        
        expectedCode = CSharpSyntaxTree.ParseText(expectedCode)
            .GetRoot()
            .NormalizeWhitespace()
            .ToFullString();
        
        var requiredCode = GetRequiredCode();
        
        var runner = new CSharpSourceGeneratorVerifier<NetworkedPropertyGenerator>.TestRunner
        {
            TestState =
            {
                Sources = { ("ExampleBehaviour.cs", code), requiredCode[0], requiredCode[1] },
                GeneratedSources = { (expectedFileName, SourceText.From(expectedCode, Encoding.UTF8)) },
            }
        };

        await runner.RunAsync();
    }
    
    [Test]
    public async Task InheritedListenerTest()
    {
        var code = """
                   using RepoSteamNetworking.API.Unity;
                   
                   namespace ExampleNamespace
                   {
                        public partial class BaseExampleBehaviour
                        {
                            [NetworkedProperty]
                            public int testNumber;
                        }
                        
                        public partial class ExampleBehaviour : BaseExampleBehaviour
                        {
                            [NetworkedProperty]
                            public int testNumber2;
                        }
                   }
                   """;

        var expectedFileName = "RepoSteamNetworking.SourceGenerator/RepoSteamNetworking.SourceGenerator.NetworkedPropertyGenerator/ExampleNamespace.ExampleBehaviour_NetworkedFieldProps.g.cs";
        var expectedCode = """
                           namespace ExampleNamespace
                           {
                                partial class BaseExampleBehaviour
                                {
                                    public int TestNumber
                                    {
                                        get => testNumber;
                                        set
                                        {
                                            if (testNumber == value)
                                            {
                                                return;
                                            }
                                            testNumber = value;
                                        }
                                    }
                                }
                                
                                partial class ExampleBehaviour
                                {
                                    public int TestNumber2
                                    {
                                        get => testNumber2;
                                        set
                                        {
                                            if (testNumber2 == value)
                                            {
                                                return;
                                            }
                                            testNumber2 = value;
                                        }
                                    }
                                }
                           }
                           """;

        expectedCode = CSharpSyntaxTree.ParseText(expectedCode)
            .GetRoot()
            .NormalizeWhitespace()
            .ToFullString();
        
        var requiredCode = GetRequiredCode();
        
        var runner = new CSharpSourceGeneratorVerifier<NetworkedPropertyGenerator>.TestRunner
        {
            TestState =
            {
                Sources = { ("ExampleBehaviours.cs", code), requiredCode[0], requiredCode[1] },
                GeneratedSources = { (expectedFileName, SourceText.From(expectedCode, Encoding.UTF8)) },
            }
        };

        await runner.RunAsync();
    }
}