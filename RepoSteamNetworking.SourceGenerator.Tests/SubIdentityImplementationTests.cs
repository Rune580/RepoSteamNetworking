namespace RepoSteamNetworking.SourceGenerator.Tests;

[TestFixture]
public class SubIdentityImplementationTests
{
    private static string[] GetRequiredCode()
    {
        var subIdentityFile = File.ReadAllText($"../../../../RepoSteamNetworking/API/Unity/ISteamNetworkSubIdentity.cs");
        var networkAttrFile = File.ReadAllText($"../../../../RepoSteamNetworking/API/Unity/NetworkedPropertyAttribute.cs");
        var rpcAttrFile = File.ReadAllText($"../../../../RepoSteamNetworking/API/Unity/RepoSteamRPCAttribute.cs");
        
        const string pluginInfoCode = """
                                      namespace ExampleNamespace
                                      {
                                          public static class MyPluginInfo
                                          {
                                              public const string PLUGIN_GUID = "com.example.myplugin";
                                          }
                                      }
                                      """;
        
        const string bepinAttrCode = """
                                     using System;
                                     namespace BepInEx
                                     {
                                        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
                                        public class BepInPlugin : Attribute
                                        {
                                            public string GUID { get; protected set; }
                                     
                                            public string Name { get; protected set; }
                                     
                                            public Version Version { get; protected set; }
                                     
                                            public BepInPlugin(string GUID, string Name, string Version)
                                            {
                                                this.GUID = GUID;
                                                this.Name = Name;
                                                try
                                                {
                                                    this.Version = new Version(Version);
                                                }
                                                catch
                                                {
                                                    this.Version = (Version) null;
                                                }
                                           }
                                     }
                                     """;
        
        const string pluginCode = """
                                  using BepInEx;
                                  namespace ExampleNamespace
                                  {
                                      [BepInPlugin("com.example.myplugin", "ExamplePlugin", "0.0.0")]
                                      public class MyPlugin { }
                                  }
                                  """;
        
        return [subIdentityFile, networkAttrFile, rpcAttrFile, pluginInfoCode, bepinAttrCode, pluginCode];
    }

    [Test]
    public async Task FromRPCAttributeWithPluginInfo()
    {
        var code = """
                   using RepoSteamNetworking.API.Unity;
                   
                   namespace ExampleNamespace
                   {
                        public partial class ExampleBehaviour
                        {
                            [RepoSteamRPC]
                            public void TestRPC() { }
                        }
                   }
                   """;

        var requiredCode = GetRequiredCode();

        var runner = new CSharpSourceGeneratorVerifier<SteamNetworkSubIdentityImplementerGenerator>.TestRunner
        {
            TestState =
            {
                Sources = {("ExampleBehaviour.cs", code), requiredCode[0], requiredCode[1], requiredCode[2], ("obj/Debug/netstandard2.1/PluginInfo.cs", requiredCode[3]) },
            }
        };

        await runner.RunAsync();
    }
    
    [Test]
    public async Task FromRPCAttributeWithBepInPlugin()
    {
        var code = """
                   using RepoSteamNetworking.API.Unity;

                   namespace ExampleNamespace
                   {
                        public partial class ExampleBehaviour
                        {
                            [RepoSteamRPC]
                            public void TestRPC() { }
                        }
                   }
                   """;

        var requiredCode = GetRequiredCode();

        var runner = new CSharpSourceGeneratorVerifier<SteamNetworkSubIdentityImplementerGenerator>.TestRunner
        {
            TestState =
            {
                Sources = {("ExampleBehaviour.cs", code), requiredCode[0], requiredCode[1], requiredCode[2], requiredCode[4], requiredCode[5] },
            }
        };

        await runner.RunAsync();
    }
}