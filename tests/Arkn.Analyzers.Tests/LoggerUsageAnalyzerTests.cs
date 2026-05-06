using Arkn.Analyzers.Analyzers;

namespace Arkn.Analyzers.Tests;

public class LoggerUsageAnalyzerTests
{
    [Fact]
    public void ConsoleWriteInArknJob_ShouldReportARK006()
    {
        var source = """
            using System.Threading.Tasks;
            public interface IArknJob { Task ExecuteAsync(); }
            public class MyJob : IArknJob
            {
                public Task ExecuteAsync()
                {
                    Console.WriteLine("hello");
                    return Task.CompletedTask;
                }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<LoggerUsageAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK006");
    }

    [Fact]
    public void ConsoleWriteInArknHttpClientSubclass_ShouldReportARK006()
    {
        var source = """
            public class ArknHttpClient { }
            public class PaymentClient : ArknHttpClient
            {
                public void Send()
                {
                    Console.WriteLine("sending");
                }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<LoggerUsageAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK006");
    }

    [Fact]
    public void IArknLoggerInArknJob_ShouldNotReport()
    {
        var source = """
            using System.Threading.Tasks;
            public interface IArknJob { Task ExecuteAsync(); }
            public interface IArknLogger { void Info(string msg); }
            public class MyJob : IArknJob
            {
                private readonly IArknLogger _logger;
                public MyJob(IArknLogger logger) { _logger = logger; }
                public Task ExecuteAsync()
                {
                    _logger.Info("hello");
                    return Task.CompletedTask;
                }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<LoggerUsageAnalyzer>(source);
        AnalyzerTestHelper.AssertNoDiagnostic(diagnostics, "ARK006");
    }

    [Fact]
    public void PlainClass_WithConsole_ShouldNotReport()
    {
        var source = """
            public class PlainService
            {
                public void DoWork()
                {
                    Console.WriteLine("not an Arkn component");
                }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<LoggerUsageAnalyzer>(source);
        AnalyzerTestHelper.AssertNoDiagnostic(diagnostics, "ARK006");
    }
}
