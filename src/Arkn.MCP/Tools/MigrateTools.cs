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
