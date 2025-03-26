using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace RepoSteamNetworking.SourceGenerator.Tests;

[TestFixture]
public class NetworkedPropertyTests
{
    private static string GetRequiredCode()
    {
        var networkAttrFile = File.ReadAllText($"../../../../RepoSteamNetworking/API/Unity/NetworkedPropertyAttribute.cs");
        
        return networkAttrFile;
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
        
        var runner = new CSharpSourceGeneratorVerifier<NetworkedPropertyGenerator>.Test
        {
            TestState =
            {
                Sources = { ("ExampleBehaviour.cs", code), GetRequiredCode() },
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
        
        var runner = new CSharpSourceGeneratorVerifier<NetworkedPropertyGenerator>.Test
        {
            TestState =
            {
                Sources = { ("ExampleBehaviour.cs", code), GetRequiredCode() },
                GeneratedSources = { (expectedFileName, SourceText.From(expectedCode, Encoding.UTF8)) },
            }
        };

        await runner.RunAsync();
    }
}