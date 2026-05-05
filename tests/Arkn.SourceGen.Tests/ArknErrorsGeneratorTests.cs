using System.Collections.Immutable;
using System.Reflection;
using Arkn.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Arkn.SourceGen.Tests;

public class ArknErrorsGeneratorTests
{
    private static Compilation CreateCompilation(string source)
    {
        // Include Arkn.Results reference so Error type resolves
        var resultsRef = MetadataReference.CreateFromFile(
            typeof(Arkn.Results.Error).Assembly.Location);

        var netCoreRef = MetadataReference.CreateFromFile(
            typeof(object).Assembly.Location);

        // Also add System.Runtime
        var runtimePath = Path.Combine(
            Path.GetDirectoryName(typeof(object).Assembly.Location)!,
            "System.Runtime.dll");
        var runtimeRef = MetadataReference.CreateFromFile(runtimePath);

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] { netCoreRef, runtimeRef, resultsRef },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static (Compilation output, ImmutableArray<Diagnostic> diagnostics)
        RunGenerator(string source)
    {
        var compilation = CreateCompilation(source);
        var generator   = new ArknErrorsGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out var diagnostics);

        return (outputCompilation, diagnostics);
    }

    [Fact]
    public void Generator_ShouldInjectAttributeSource()
    {
        var (output, _) = RunGenerator("// empty");

        var attrTree = output.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("ArknSourceGenAttributes"));

        Assert.NotNull(attrTree);
        var text = attrTree!.GetText().ToString();
        Assert.Contains("ArknErrorsAttribute", text);
        Assert.Contains("ArknErrorCodeAttribute", text);
    }

    [Fact]
    public void Generator_WithArknErrorsClass_ShouldGenerateImplementation()
    {
        const string source = """
            using Arkn.SourceGen;
            using Arkn.Results;

            [ArknErrors]
            public static partial class UserErrors
            {
                [ArknErrorCode("NotFound", "User was not found")]
                public static partial Error NotFound(string? detail = null);
            }
            """;

        var (output, _) = RunGenerator(source);

        var generated = output.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("UserErrors"));

        Assert.NotNull(generated);
        var text = generated!.GetText().ToString();
        Assert.Contains("UserErrors.NotFound", text);
        Assert.Contains("Error.NotFound", text);
        Assert.Contains("User was not found", text);
    }

    [Fact]
    public void Generator_ShouldGenerateAllMethods()
    {
        const string source = """
            using Arkn.SourceGen;
            using Arkn.Results;

            namespace MyApp.Errors;

            [ArknErrors]
            public static partial class OrderErrors
            {
                [ArknErrorCode("NotFound", "Order not found")]
                public static partial Error NotFound(string? detail = null);

                [ArknErrorCode("Conflict", "Order already processed")]
                public static partial Error AlreadyProcessed(string? detail = null);

                [ArknErrorCode("Validation", "Invalid order state")]
                public static partial Error InvalidState(string? detail = null);
            }
            """;

        var (output, _) = RunGenerator(source);

        var generated = output.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("OrderErrors"));

        Assert.NotNull(generated);
        var text = generated!.GetText().ToString();

        Assert.Contains("OrderErrors.NotFound",         text);
        Assert.Contains("OrderErrors.AlreadyProcessed", text);
        Assert.Contains("OrderErrors.InvalidState",     text);
        Assert.Contains("Error.NotFound",   text);
        Assert.Contains("Error.Conflict",   text);
        Assert.Contains("Error.Validation", text);
    }

    [Fact]
    public void Generator_ShouldRespectNamespace()
    {
        const string source = """
            using Arkn.SourceGen;
            using Arkn.Results;

            namespace Company.Domain.Errors;

            [ArknErrors]
            public static partial class ProductErrors
            {
                [ArknErrorCode("NotFound", "Product not found")]
                public static partial Error NotFound(string? detail = null);
            }
            """;

        var (output, _) = RunGenerator(source);

        var generated = output.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("ProductErrors"));

        Assert.NotNull(generated);
        var text = generated!.GetText().ToString();
        Assert.Contains("namespace Company.Domain.Errors", text);
    }

    [Fact]
    public void Generator_ClassWithoutAttribute_ShouldNotGenerate()
    {
        const string source = """
            using Arkn.Results;

            public static partial class PlainClass
            {
                public static partial Error NotFound(string? detail = null);
            }
            """;

        var (output, _) = RunGenerator(source);

        var generated = output.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("PlainClass.g.cs"));

        Assert.Null(generated);
    }
}
