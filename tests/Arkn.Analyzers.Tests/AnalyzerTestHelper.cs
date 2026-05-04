using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arkn.Analyzers.Tests;

/// <summary>
/// Lightweight helper to run Roslyn analyzers against C# source snippets.
/// No external testing frameworks required — compatible with any xunit version.
/// </summary>
internal static class AnalyzerTestHelper
{
    public static ImmutableArray<Diagnostic> GetDiagnostics<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new TAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
    }

    public static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
    {
        Assert.Contains(diagnostics, d => d.Id == diagnosticId);
    }

    public static void AssertNoDiagnostic(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
    {
        Assert.DoesNotContain(diagnostics, d => d.Id == diagnosticId);
    }
}
