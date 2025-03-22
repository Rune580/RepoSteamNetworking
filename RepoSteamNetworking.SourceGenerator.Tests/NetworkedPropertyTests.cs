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
                   
                   public partial class ExampleBehaviour
                   {
                       [NetworkedProperty]
                       public int testNumber;
                   }
                   """;

        var expectedCode = """
                           using RepoSteamNetworking.API.Unity;
                           
                           public partial class ExampleBehaviour
                           {
                               [NetworkedProperty]
                               public int testNumber;
                           }
                           """;

        var runner = new CSharpSourceGeneratorVerifier<NetworkedPropertyGenerator>.Test
        {
            TestState =
            {
                Sources = { code, GetRequiredCode() },
                // GeneratedSources = {  }
            }
        };

        await runner.RunAsync();
    }
}