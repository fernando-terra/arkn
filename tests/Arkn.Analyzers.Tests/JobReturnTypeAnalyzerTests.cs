using Arkn.Analyzers.Analyzers;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Arkn.Analyzers.Analyzers.JobReturnTypeAnalyzer>;

namespace Arkn.Analyzers.Tests;

public class JobReturnTypeAnalyzerTests
{
    [Fact]
    public async Task CorrectReturnType_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            using Arkn.Jobs.Abstractions;
            using Arkn.Jobs.Models;
            using Arkn.Results;
            class MyJob : IArknJob {
                public Task<Result> ExecuteAsync(ArknJobContext ctx) => Task.FromResult(Result.Success());
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task WrongReturnType_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            using Arkn.Jobs.Abstractions;
            using Arkn.Jobs.Models;
            class MyJob : IArknJob {
                public {|ARK004:Task<bool>|} ExecuteAsync(ArknJobContext ctx) => Task.FromResult(true);
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
