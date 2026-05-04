using Arkn.Analyzers.Analyzers;

namespace Arkn.Analyzers.Tests;

public class JobReturnTypeAnalyzerTests
{
    private const string IArknJobBoilerplate = """

        public interface IArknJob { }
        public class Result { }
        """;

    [Fact]
    public void CorrectReturnType_NoDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;

            public class MyJob : IArknJob
            {
                public async Task<Result> ExecuteAsync(CancellationToken ct)
                {
                    return new Result();
                }
            }
            """ + IArknJobBoilerplate;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<JobReturnTypeAnalyzer>(source);
        AnalyzerTestHelper.AssertNoDiagnostic(diagnostics, "ARK004");
    }

    [Fact]
    public void WrongReturnType_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;

            public class BadJob : IArknJob
            {
                public async Task ExecuteAsync(CancellationToken ct)
                {
                }
            }
            """ + IArknJobBoilerplate;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<JobReturnTypeAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK004");
    }
}
