using Arkn.Analyzers.Analyzers;

namespace Arkn.Analyzers.Tests;

public class ErrorCodeNamingAnalyzerTests
{
    [Fact]
    public void ValidCode_NoDiagnostic()
    {
        var source = """
            public class MyService
            {
                public void Do()
                {
                    var e = Error.NotFound("User.NotFound", "msg");
                }
            }
            public static class Error
            {
                public static object NotFound(string code, string msg) => null!;
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<ErrorCodeNamingAnalyzer>(source);
        AnalyzerTestHelper.AssertNoDiagnostic(diagnostics, "ARK002");
    }

    [Fact]
    public void ValidCode_MultipleSegments_NoDiagnostic()
    {
        var source = """
            public class MyService
            {
                public void Do()
                {
                    var e = Error.Validation("Order.Item.QuantityInvalid", "msg");
                }
            }
            public static class Error
            {
                public static object Validation(string code, string msg) => null!;
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<ErrorCodeNamingAnalyzer>(source);
        AnalyzerTestHelper.AssertNoDiagnostic(diagnostics, "ARK002");
    }

    [Fact]
    public void InvalidCode_SingleSegment_ReportsDiagnostic()
    {
        var source = """
            public class MyService
            {
                public void Do()
                {
                    var e = Error.NotFound("usernotfound", "msg");
                }
            }
            public static class Error
            {
                public static object NotFound(string code, string msg) => null!;
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<ErrorCodeNamingAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK002");
    }

    [Fact]
    public void InvalidCode_LowercaseStart_ReportsDiagnostic()
    {
        var source = """
            public class MyService
            {
                public void Do()
                {
                    var e = Error.Failure("user.notFound", "msg");
                }
            }
            public static class Error
            {
                public static object Failure(string code, string msg) => null!;
            }
            """;

        var diagnostics = AnalyzerTestHelper.GetDiagnostics<ErrorCodeNamingAnalyzer>(source);
        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "ARK002");
    }
}
