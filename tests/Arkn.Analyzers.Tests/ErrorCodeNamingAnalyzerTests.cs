using Arkn.Analyzers.Analyzers;
using Arkn.Analyzers.Resources;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Arkn.Analyzers.Analyzers.ErrorCodeNamingAnalyzer>;

namespace Arkn.Analyzers.Tests;

public class ErrorCodeNamingAnalyzerTests
{
    [Fact]
    public async Task ValidCode_NoDiagnostic()
    {
        const string source = """
            using Arkn.Results;
            class C {
                void M() { var e = Error.NotFound("User.NotFound", "msg"); }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidCode_SingleSegment_ReportsDiagnostic()
    {
        const string source = """
            using Arkn.Results;
            class C {
                void M() { var e = Error.NotFound({|ARK002:"UserNotFound"|}, "msg"); }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task InvalidCode_LowercaseStart_ReportsDiagnostic()
    {
        const string source = """
            using Arkn.Results;
            class C {
                void M() { var e = Error.Validation({|ARK002:"user.notFound"|}, "msg"); }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ValidCode_MultipleSegments_NoDiagnostic()
    {
        const string source = """
            using Arkn.Results;
            class C {
                void M() { var e = Error.Conflict("Order.Payment.Duplicate", "msg"); }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
