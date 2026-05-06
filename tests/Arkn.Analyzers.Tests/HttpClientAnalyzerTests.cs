using Arkn.Analyzers.Analyzers;

namespace Arkn.Analyzers.Tests;

public class HttpClientAnalyzerTests
{
    [Fact]
    public void NewHttpClient_ShouldReportARK005()
    {
        var source = """
            using System.Net.Http;
            public class MyService
            {
                public void DoWork()
                {
                    var client = new HttpClient();
                }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<HttpClientAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK005");
    }

    [Fact]
    public void HttpClientField_ShouldReportARK005()
    {
        var source = """
            using System.Net.Http;
            public class MyService
            {
                private readonly HttpClient _http;
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<HttpClientAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK005");
    }

    [Fact]
    public void HttpClientConstructorParam_ShouldReportARK005()
    {
        var source = """
            using System.Net.Http;
            public class MyService
            {
                public MyService(HttpClient http) { }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<HttpClientAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK005");
    }

    [Fact]
    public void ArknHttpClientSubclass_ShouldNotReport()
    {
        var source = """
            using System.Net.Http;
            public class ArknHttpClient { }
            public class PaymentClient : ArknHttpClient
            {
                public void DoWork()
                {
                    var client = new HttpClient();
                }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<HttpClientAnalyzer>(source);
        AnalyzerTestHelper.AssertNoDiagnostic(diagnostics, "ARK005");
    }

    [Fact]
    public void PlainClass_WithNoHttpClient_ShouldNotReport()
    {
        var source = """
            public class MyService
            {
                public void DoWork() { }
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<HttpClientAnalyzer>(source);
        AnalyzerTestHelper.AssertNoDiagnostic(diagnostics, "ARK005");
    }
}
