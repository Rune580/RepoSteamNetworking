using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace RepoSteamNetworking.SourceGenerator.Tests;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public class TestRunner : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        public TestRunner()
        {
            
        }
    }
}