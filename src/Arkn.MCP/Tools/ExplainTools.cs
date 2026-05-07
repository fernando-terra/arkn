using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Arkn.MCP.Tools;

/// <summary>
/// explain_error — takes an Arkn error code and returns a natural-language explanation
/// with usage examples.
/// </summary>
[McpServerToolType]
public static class ExplainTools
{
    private static readonly IReadOnlyDictionary<string, (string ErrorType, string Meaning, string HttpStatus, string WhenToUse)> _knownPatterns
        = new Dictionary<string, (string, string, string, string)>(StringComparer.OrdinalIgnoreCase)
    {
        ["NotFound"]     = ("Error.NotFound",    "The requested resource does not exist",                                   "404 Not Found",             "when a database lookup, file read or lookup by id returns nothing"),
        ["Invalid"]      = ("Error.Validation",  "The input data failed validation rules",                                  "400 Bad Request",           "when a field is missing, malformed or out of range"),
        ["Validation"]   = ("Error.Validation",  "The input data failed validation rules",                                  "400 Bad Request",           "when a field is missing, malformed or out of range"),
        ["Conflict"]     = ("Error.Conflict",    "A conflicting resource or state already exists",                          "409 Conflict",              "when creating a resource that already exists, or state transition is illegal"),
        ["Unauthorized"] = ("Error.Unauthorized","The caller is not authenticated",                                         "401 Unauthorized",          "when there is no valid authentication token or it has expired"),
        ["Forbidden"]    = ("Error.Forbidden",   "The caller is authenticated but lacks permission",                        "403 Forbidden",             "when the user is logged in but does not have the required role or permission"),
        ["Failure"]      = ("Error.Failure",     "A generic unclassified failure (internal error or third-party failure)",  "500 Internal Server Error", "when an unexpected exception occurs or an external service returns an error"),
        ["TimedOut"]     = ("Error.Failure",     "The operation exceeded its allowed time",                                 "504 Gateway Timeout",       "when an async operation or job run exceeds its configured timeout"),
        ["AlreadyExists"]= ("Error.Conflict",    "The resource already exists",                                             "409 Conflict",              "when a unique constraint is violated"),
        ["Expired"]      = ("Error.Unauthorized","The token or resource has expired",                                       "401 Unauthorized",          "when a session token, OTP or link is no longer valid"),
        ["Inactive"]     = ("Error.Validation",  "The resource exists but is not in an active state",                       "422 Unprocessable Entity",  "when acting on a deactivated or suspended entity"),
    };

    [McpServerTool, Description(
        "Explains an Arkn error code in natural language. " +
        "Accepts codes in 'Namespace.Reason' format (e.g. 'User.NotFound', 'Payment.Conflict') " +
        "and returns: the error type, meaning, recommended HTTP status, when to use it, and a usage example.")]
    public static string ExplainError(
        [Description("Arkn error code in 'Namespace.Reason' format, e.g. 'User.NotFound', 'Order.InvalidState'")] string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return "Error: error code cannot be empty.";

        // ── Parse namespace and reason ────────────────────────────────────────
        var dotIdx = errorCode.IndexOf('.');
        string ns, reason;

        if (dotIdx < 0)
        {
            ns     = errorCode;
            reason = "Failure";
        }
        else
        {
            ns     = errorCode[..dotIdx];
            reason = errorCode[(dotIdx + 1)..];
        }

        ns     = char.ToUpperInvariant(ns[0]) + ns[1..];
        reason = reason.Length > 0 ? char.ToUpperInvariant(reason[0]) + reason[1..] : "Failure";

        // ── Validate ARK002 convention ────────────────────────────────────────
        if (!Regex.IsMatch(errorCode, @"^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)+$"))
        {
            return $"""
                ⚠️  '{errorCode}' does not follow the ARK002 naming convention.

                Error codes must be PascalCase with a dot separator: 'Namespace.Reason'

                Examples:
                  ✅  User.NotFound
                  ✅  Payment.InsufficientFunds
                  ✅  Order.AlreadyCancelled
                  ❌  {errorCode}

                Fix: Use '{char.ToUpper(errorCode[0])}{errorCode[1..].Replace(".", ".")}'
                """;
        }

        // ── Look up known reason suffix ───────────────────────────────────────
        var matched = _knownPatterns
            .FirstOrDefault(kv => reason.Contains(kv.Key, StringComparison.OrdinalIgnoreCase)
                               || reason.Equals(kv.Key, StringComparison.OrdinalIgnoreCase));

        var (errorFactory, meaning, httpStatus, whenToUse) = matched.Key is not null
            ? matched.Value
            : ("Error.Failure", $"A failure specific to the {ns} domain", "500 Internal Server Error", "when a domain-specific error condition is encountered");

        // ── Build explanation ─────────────────────────────────────────────────
        var sb = new StringBuilder();
        sb.AppendLine($"# {errorCode}");
        sb.AppendLine();
        sb.AppendLine($"| Property | Value |");
        sb.AppendLine($"|---|---|");
        sb.AppendLine($"| **Error factory** | `{errorFactory}` |");
        sb.AppendLine($"| **Meaning** | {meaning} |");
        sb.AppendLine($"| **HTTP status** | {httpStatus} |");
        sb.AppendLine($"| **Use when** | {whenToUse} |");
        sb.AppendLine($"| **Namespace** | `{ns}` (the domain or aggregate owning this error) |");
        sb.AppendLine($"| **Reason** | `{reason}` (the specific failure case) |");
        sb.AppendLine();
        sb.AppendLine("## Usage example");
        sb.AppendLine();
        sb.AppendLine("### 1. Define in an ErrorGroup (recommended with `Arkn.SourceGen`)");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("using Arkn.Results;");
        sb.AppendLine("using Arkn.SourceGen;");
        sb.AppendLine();
        sb.AppendLine("[ArknErrors]");
        sb.AppendLine($"public static partial class {ns}Errors");
        sb.AppendLine("{");
        sb.AppendLine($"    [ArknErrorCode(\"{reason}\", \"{meaning}\")]");
        sb.AppendLine($"    public static partial Error {reason}(string? detail = null);");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("### 2. Return from domain/application method");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine($"public async Task<Result<{ns}Dto>> GetAsync(Guid id)");
        sb.AppendLine("{");
        sb.AppendLine($"    var entity = await _repo.FindAsync(id);");
        sb.AppendLine($"    if (entity is null) return {ns}Errors.{reason}($\"{{id}} was not found.\");");
        sb.AppendLine($"    return new {ns}Dto(entity);");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("### 3. Handle at API boundary");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine($"var result = await svc.GetAsync(id);");
        sb.AppendLine("return result.Match(");
        sb.AppendLine($"    onSuccess: dto   => Results.Ok(dto),");
        sb.AppendLine( "    onFailure: error => error.Type switch");
        sb.AppendLine( "    {");

        // Map to HTTP response based on error type
        var httpMapping = httpStatus switch
        {
            var s when s.StartsWith("404") => "ErrorType.NotFound   => Results.NotFound(new { error.Code, error.Message }),",
            var s when s.StartsWith("400") => "ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),",
            var s when s.StartsWith("409") => "ErrorType.Conflict   => Results.Conflict(new { error.Code, error.Message }),",
            var s when s.StartsWith("401") => "ErrorType.Unauthorized => Results.Unauthorized(),",
            var s when s.StartsWith("403") => "ErrorType.Forbidden  => Results.Forbid(),",
            _                              => "_                    => Results.Problem(error.Message),",
        };

        sb.AppendLine($"        {httpMapping}");
        sb.AppendLine( "        _                    => Results.Problem(error.Message)");
        sb.AppendLine( "    });");
        sb.AppendLine("```");

        return sb.ToString();
    }
}
