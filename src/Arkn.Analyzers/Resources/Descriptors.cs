using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Arkn.Analyzers.Resources;

internal static class Descriptors
{
    private const string Category = "Arkn";

    public static readonly DiagnosticDescriptor ARK001_DomainMethodShouldReturnResult = new(
        id:                 "ARK001",
        title:              "Domain method should return Result or Result<T>",
        messageFormat:      "Method '{0}' in a Domain type should return 'Result' or 'Result<T>' instead of throwing exceptions",
        category:           Category,
        defaultSeverity:    DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:        "Methods on Domain entities and value objects should use the Result pattern instead of throwing exceptions, making failures explicit in the type system.",
        helpLinkUri:        "https://github.com/fernando-terra/arkn/blob/main/docs/analyzers.md#ARK001");

    public static readonly DiagnosticDescriptor ARK002_ErrorCodeMustFollowNamingConvention = new(
        id:                 "ARK002",
        title:              "Error code must follow 'Namespace.Reason' pattern",
        messageFormat:      "Error code '{0}' does not follow the 'Namespace.Reason' naming convention (e.g. 'User.NotFound', 'Order.InvalidState').",
        category:           Category,
        defaultSeverity:    DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:        "Error codes must be dot-separated PascalCase identifiers with at least two segments (e.g. 'User.NotFound'). This ensures consistency and machine-readability.",
        helpLinkUri:        "https://github.com/fernando-terra/arkn/blob/main/docs/analyzers.md#ARK002");

    public static readonly DiagnosticDescriptor ARK003_ResultMustNotBeSilentlyDiscarded = new(
        id:                 "ARK003",
        title:              "Result must not be silently discarded",
        messageFormat:      "The Result returned by '{0}' is being discarded. Handle the result or assign it to a variable.",
        category:           Category,
        defaultSeverity:    DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:        "Discarding a Result means ignoring potential failures. Always handle both success and failure paths using Match, Map, Bind or an explicit check.",
        helpLinkUri:        "https://github.com/fernando-terra/arkn/blob/main/docs/analyzers.md#ARK003");

    public static readonly DiagnosticDescriptor ARK004_JobExecuteAsyncMustReturnTaskResult = new(
        id:                 "ARK004",
        title:              "IArknJob.ExecuteAsync must return Task<Result>",
        messageFormat:      "Method '{0}' implementing IArknJob.ExecuteAsync must have return type 'Task<Result>'",
        category:           Category,
        defaultSeverity:    DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:        "All IArknJob implementations must return Task<Result> from ExecuteAsync. Using Task, void, or any other return type breaks the Arkn job runner contract.",
        helpLinkUri:        "https://github.com/fernando-terra/arkn/blob/main/docs/analyzers.md#ARK004");

    public static readonly DiagnosticDescriptor ARK005_AvoidRawHttpClient = new(
        id:                 "ARK005",
        title:              "Avoid using raw HttpClient",
        messageFormat:      "'{0}' uses HttpClient directly. Inject IArknHttp and extend ArknHttpClient instead.",
        category:           "Arkn.Http",
        defaultSeverity:    DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:        "Direct use of HttpClient bypasses Arkn's retry, timeout and Result-based error handling. " +
                            "Extend ArknHttpClient and register via AddArknHttp<TClient>() instead.",
        helpLinkUri:        "https://github.com/fernando-terra/arkn/blob/main/docs/analyzers.md#ARK005");

    public static readonly DiagnosticDescriptor ARK006_PreferIArknLogger = new(
        id:                 "ARK006",
        title:              "Prefer IArknLogger over ILogger or Console in Arkn components",
        messageFormat:      "'{0}' uses {1} in an Arkn-managed component. Use IArknLogger for consistent structured logging.",
        category:           "Arkn.Logging",
        defaultSeverity:    DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:        "Using MEL ILogger or Console.Write in classes that implement IArknJob or extend ArknHttpClient " +
                            "bypasses the Arkn logging pipeline. Inject IArknLogger instead.",
        helpLinkUri:        "https://github.com/fernando-terra/arkn/blob/main/docs/analyzers.md#ARK006");
}
