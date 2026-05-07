using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Arkn.MCP.Tools;

[McpServerToolType]
public static class MigrateTools
{
    [McpServerTool, Description("Refactors a C# method that uses throw/try-catch into one that returns Result or Result<T> using the Arkn Result pattern.")]
    public static string MigrateExceptionToResult(
        [Description("C# method source code that uses throw or try-catch")] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Error: code cannot be empty.";

        var changes = new List<string>();
        var result  = code;

        // 1. Add using if missing
        if (!result.Contains("using Arkn.Results"))
        {
            result = "using Arkn.Results;\n" + result;
            changes.Add("Added 'using Arkn.Results;'");
        }

        // 2. void return → Result
        if (Regex.IsMatch(result, @"\bvoid\b\s+\w+\s*\("))
        {
            result = Regex.Replace(result, @"\bvoid\b(\s+\w+\s*\()", "Result$1");
            changes.Add("Changed return type from void to Result");
        }

        // 3. T return (non-void, non-Result) → Result<T>
        result = Regex.Replace(result,
            @"\b(public|private|protected|internal)\s+((?!Result|Task|void|static|sealed|readonly|override|async)\w+)\s+(\w+\s*\([^)]*\))",
            m => {
                var modifier = m.Groups[1].Value;
                var retType  = m.Groups[2].Value;
                var rest     = m.Groups[3].Value;
                changes.Add($"Changed return type from {retType} to Result<{retType}>");
                return $"{modifier} Result<{retType}> {rest}";
            });

        // 4. throw new XException(msg) → return Result.Failure(...)
        result = Regex.Replace(result,
            @"throw\s+new\s+(\w+Exception)\s*\(([^)]*)\)\s*;",
            m => {
                var exType = m.Groups[1].Value;
                var msg    = m.Groups[2].Value.Trim().Trim('"');
                changes.Add($"Replaced 'throw new {exType}(...)' with Result.Failure(...)");
                return $"return Result.Failure(Error.Failure(\"Domain.{exType.Replace("Exception", "")}\", {(string.IsNullOrWhiteSpace(msg) ? $"\"{exType} occurred\"" : m.Groups[2].Value)}));";
            });

        // 5. throw; (rethrow) → return Result.Failure(...)
        result = Regex.Replace(result, @"\bthrow\s*;",
            "return Result.Failure(Error.Failure(\"Domain.Unexpected\", ex.Message));");

        // 6. catch blocks: add Result.Failure if missing
        result = Regex.Replace(result,
            @"catch\s*\(Exception\s+(\w+)\)\s*\{(\s*)\}",
            m => {
                var varName = m.Groups[1].Value;
                var ws      = m.Groups[2].Value;
                changes.Add("Added Result.Failure in empty catch block");
                return $"catch (Exception {varName}){{{ws}    return Result.Failure(Error.Failure(\"Domain.Unexpected\", {varName}.Message));{ws}}}";
            });

        // 7. async Task → async Task<Result> if returning Result
        if (result.Contains("Result.Failure") || result.Contains("Result.Success"))
        {
            result = Regex.Replace(result,
                @"\basync\s+Task\b(?!<)",
                "async Task<Result>");
            if (result.Contains("async Task<Result>"))
                changes.Add("Changed 'async Task' to 'async Task<Result>'");
        }

        var sb = new StringBuilder();
        sb.AppendLine("// ── Refactored by Arkn.MCP migrate_exception_to_result ──────────────────");
        sb.AppendLine(result.TrimEnd());
        sb.AppendLine();
        sb.AppendLine("// ── Changes applied ──────────────────────────────────────────────────────");
        if (changes.Count == 0)
            sb.AppendLine("// No automatic transformations applied. Review the code manually.");
        else
            foreach (var c in changes) sb.AppendLine($"// ✓ {c}");

        return sb.ToString();
    }

    [McpServerTool, Description(
        "Converts a C# catch block into a Result.Failure return with the semantically correct Arkn ErrorType. " +
        "Analyses the exception type and message pattern to choose NotFound, Validation, Conflict, Unauthorized, Forbidden or Failure.")]
    public static string MigrateException(
        [Description("A C# catch block, e.g. 'catch (ArgumentNullException ex) { throw; }' or 'catch (Exception ex) { _logger.Log(ex); }'")] string catchBlock,
        [Description("Error code to use (Namespace.Reason format). Leave empty to auto-generate from the exception type.")] string errorCode = "")
    {
        if (string.IsNullOrWhiteSpace(catchBlock))
            return "Error: catch block cannot be empty.";

        // ── Extract exception type and variable ───────────────────────────────
        var catchMatch = Regex.Match(catchBlock,
            @"catch\s*\(\s*(\w+(?:<[^>]+>)?)(?:\s+(\w+))?\s*\)");

        var exType  = catchMatch.Success ? catchMatch.Groups[1].Value : "Exception";
        var exVar   = catchMatch.Success && catchMatch.Groups[2].Length > 0
                        ? catchMatch.Groups[2].Value : "ex";

        // ── Classify exception to Arkn ErrorType ─────────────────────────────
        var (suggestedReason, arkErrorType) = RefactorTools.ClassifyException(exType);

        // ── Resolve error code ────────────────────────────────────────────────
        string code;
        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            code = errorCode;
        }
        else
        {
            // Infer domain from context (class names, method names in catch block)
            var domainMatch = Regex.Match(catchBlock, @"(\w+(?:Service|Repository|Handler|Manager))");
            var domain = domainMatch.Success
                ? Regex.Replace(domainMatch.Groups[1].Value, "(Service|Repository|Handler|Manager)$", "")
                : "Domain";
            domain = char.ToUpperInvariant(domain[0]) + domain[1..];
            code   = $"{domain}.{suggestedReason}";
        }

        // ── Detect what the existing catch body does ──────────────────────────
        var bodyMatch  = Regex.Match(catchBlock, @"\{(.*?)\}$", RegexOptions.Singleline);
        var bodyRaw    = bodyMatch.Success ? bodyMatch.Groups[1].Value.Trim() : "";

        // Detect: rethrow, empty, log-only, or has return
        var hasRethrow = Regex.IsMatch(bodyRaw, @"\bthrow\s*;");
        var hasReturn  = Regex.IsMatch(bodyRaw, @"\breturn\b");
        var isEmpty    = string.IsNullOrWhiteSpace(bodyRaw) || bodyRaw == "{ }";
        var isLogOnly  = !hasRethrow && !hasReturn &&
                         Regex.IsMatch(bodyRaw, @"\b(logger|_logger|log|Log|Logger|Console)\b");

        // ── Build the migrated catch block ────────────────────────────────────
        var sb = new StringBuilder();
        sb.AppendLine("// ── Migrated by Arkn.MCP migrate_exception ──────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"catch ({exType} {exVar})");
        sb.AppendLine("{");

        // Preserve non-trivial logging if present
        if (isLogOnly)
        {
            var logLines = bodyRaw
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l));
            foreach (var l in logLines)
                sb.AppendLine($"    {l}");
        }

        sb.AppendLine($"    return Result.Failure(Error.{arkErrorType}(\"{code}\", {exVar}.Message));");
        sb.AppendLine("}");
        sb.AppendLine();

        // ── Explanation ───────────────────────────────────────────────────────
        sb.AppendLine("// ── Why this ErrorType? ─────────────────────────────────────────────────");
        sb.AppendLine($"// Exception type : {exType}");
        sb.AppendLine($"// → Arkn ErrorType: Error.{arkErrorType}  (reason: {ExplainChoice(exType, arkErrorType)})");
        sb.AppendLine($"// Error code      : \"{code}\"  (ARK002-compliant Namespace.Reason)");
        sb.AppendLine();
        sb.AppendLine("// ── Before / After summary ──────────────────────────────────────────────");

        if (isEmpty)        sb.AppendLine("// Before: empty catch — exception was silently swallowed (ARK008)");
        else if (hasRethrow) sb.AppendLine("// Before: rethrow — exception propagated as unhandled (ARK007 / ARK008)");
        else if (isLogOnly)  sb.AppendLine("// Before: log-only catch — exception swallowed after logging (ARK008)");
        else if (hasReturn)  sb.AppendLine("// Before: partial return — may have been incomplete");
        else                 sb.AppendLine("// Before: custom body — review preserved lines above");

        sb.AppendLine($"// After : Result.Failure returned — caller receives a typed, inspectable error");

        return sb.ToString();
    }

    private static string ExplainChoice(string exType, string arkType) => (exType, arkType) switch
    {
        (_, "NotFound")     => "indicates the requested entity was not found",
        (_, "Validation")   => "indicates bad/missing input data",
        (_, "Conflict")     => "indicates a state conflict or duplicate",
        (_, "Unauthorized") => "indicates authentication failure",
        (_, "Forbidden")    => "indicates authorisation failure",
        _                   => "generic unclassified failure — consider a more specific type",
    };

    [McpServerTool, Description("Refactors C# code using raw HttpClient into a typed ArknHttpClient with Result-based error handling.")]
    public static string MigrateHttpClientToArkn(
        [Description("C# code that uses HttpClient directly")] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Error: code cannot be empty.";

        var changes = new List<string>();

        // Extract class name hint
        var classMatch = Regex.Match(code, @"class\s+(\w+)");
        var className  = classMatch.Success ? classMatch.Groups[1].Value : "MyApi";
        var clientName = className.EndsWith("Client") ? className : $"{className}Client";

        // Detect base URL if present
        var urlMatch = Regex.Match(code, @"""(https?://[^""]+)""");
        var baseUrl  = urlMatch.Success ? urlMatch.Groups[1].Value : "https://api.example.com";

        // Build typed client
        var sb = new StringBuilder();
        sb.AppendLine("using Arkn.Http;");
        sb.AppendLine("using Arkn.Results;");
        sb.AppendLine();
        sb.AppendLine($"// ── Generated typed client (replace {className}) ─────────────────────");
        sb.AppendLine($"public sealed class {clientName}(IArknHttp http)");
        sb.AppendLine($"    : ArknHttpClient(http, \"{baseUrl}\")");
        sb.AppendLine("{");

        // Transform HttpClient calls to ArknHttpClient methods
        var methods = new List<string>();

        foreach (Match m in Regex.Matches(code, @"GetAsync\s*\(\s*([^)]+)\)"))
        {
            methods.Add($"    public Task<Result<ResponseDto>> GetAsync() =>\n        GetAs<ResponseDto>({m.Groups[1].Value.Trim()});");
            changes.Add("GetAsync → GetAs<T>");
        }
        foreach (Match m in Regex.Matches(code, @"PostAsJsonAsync\s*\(\s*([^,)]+),\s*([^)]+)\)"))
        {
            methods.Add($"    public Task<Result<ResponseDto>> CreateAsync(RequestDto body) =>\n        PostAs<ResponseDto>({m.Groups[1].Value.Trim()}, body);");
            changes.Add("PostAsJsonAsync → PostAs<T>");
        }
        foreach (Match m in Regex.Matches(code, @"PutAsJsonAsync\s*\(\s*([^,)]+),\s*([^)]+)\)"))
        {
            methods.Add($"    public Task<Result<ResponseDto>> UpdateAsync(RequestDto body) =>\n        PutAs<ResponseDto>({m.Groups[1].Value.Trim()}, body);");
            changes.Add("PutAsJsonAsync → PutAs<T>");
        }
        foreach (Match m in Regex.Matches(code, @"DeleteAsync\s*\(\s*([^)]+)\)"))
        {
            methods.Add($"    public Task<Result> DeleteAsync(Guid id) =>\n        Delete({m.Groups[1].Value.Trim()});");
            changes.Add("DeleteAsync → Delete");
        }

        if (methods.Count == 0)
            methods.Add("    // TODO: add typed methods — see scaffold_http_client tool for examples");

        sb.AppendLine(string.Join("\n\n", methods));
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("// ── DI Registration ───────────────────────────────────────────────────────");
        sb.AppendLine("//");
        sb.AppendLine($"// builder.Services");
        sb.AppendLine($"//     .AddArknHttp<{clientName}>(\"{baseUrl}\")");
        sb.AppendLine("//     .WithRetry(maxAttempts: 3)");
        sb.AppendLine("//     .WithTimeout(TimeSpan.FromSeconds(30));");
        sb.AppendLine();
        sb.AppendLine("// ── Changes applied ──────────────────────────────────────────────────────");
        changes.Add("Replaced HttpClient injection with IArknHttp");
        foreach (var c in changes) sb.AppendLine($"// ✓ {c}");

        return sb.ToString();
    }
}
