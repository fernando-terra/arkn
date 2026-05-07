using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace Arkn.MCP.Tools;

/// <summary>
/// review_pattern — validates C# code against ARK001-ARK008 and returns a
/// human-readable code review with severity ratings, explanations and fix suggestions.
/// </summary>
[McpServerToolType]
public static class ReviewTools
{
    private static readonly IReadOnlyDictionary<string, (string Severity, string Description, string FixHint)> _ruleInfo
        = new Dictionary<string, (string, string, string)>
    {
        ["ARK001"] = ("⚠️ Warning", "Domain method returns void or Task instead of Result",
            "Change the return type to Result or Result<T> to make failures explicit."),
        ["ARK002"] = ("⚠️ Warning", "Error code does not follow the 'Namespace.Reason' convention",
            "Use PascalCase with a dot separator, e.g. 'User.NotFound' or 'Order.AlreadyCancelled'."),
        ["ARK003"] = ("⚠️ Warning", "Result or Result<T> is silently discarded",
            "Consume the result with .Match(), .Bind(), or check .IsSuccess before accessing .Value."),
        ["ARK004"] = ("❌ Error",   "IArknJob.ExecuteAsync must return Task<Result>",
            "Change the return type from Task (or void) to Task<Result>."),
        ["ARK005"] = ("⚠️ Warning", "Raw HttpClient used instead of ArknHttpClient",
            "Register a typed client: builder.Services.AddArknHttp<TClient>(baseUrl)"),
        ["ARK006"] = ("⚠️ Warning", "MEL ILogger or Console used instead of IArknLogger",
            "Inject IArknLogger and use .Info(), .Warning(), .Error() for structured, sink-routed output."),
        ["ARK007"] = ("⚠️ Warning", "throw new in a domain method",
            "Replace with return Result.Failure(Error.*(code, message)) — no exceptions in domain logic."),
        ["ARK008"] = ("⚠️ Warning", "catch block swallows the exception silently",
            "Return Result.Failure(Error.Failure(code, ex.Message)) to propagate the failure explicitly."),
    };

    [McpServerTool, Description(
        "Reviews a C# code snippet against all Arkn patterns (ARK001–ARK008). " +
        "Returns a formatted code review with severity, descriptions, line numbers and actionable fix suggestions. " +
        "Unlike validate_pattern (which returns raw JSON), this returns a human-readable markdown review.")]
    public static string ReviewPattern(
        [Description("C# source code to review")] string code,
        [Description("Optional context label shown in the review header, e.g. 'OrderService.cs'")] string fileName = "")
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Error: code cannot be empty.";

        // Run the existing validator to get raw violations
        var rawJson    = ValidateTools.ValidatePattern(code);
        var violations = JsonSerializer.Deserialize<List<JsonElement>>(rawJson) ?? [];

        var fileLabel = string.IsNullOrWhiteSpace(fileName) ? "Code Review" : $"Code Review — {fileName}";
        var sb        = new StringBuilder();

        // ── Header ────────────────────────────────────────────────────────────
        sb.AppendLine($"# {fileLabel}");
        sb.AppendLine();

        if (violations.Count == 0)
        {
            sb.AppendLine("✅ **No Arkn pattern violations found.** This code follows ARK001–ARK008.");
            sb.AppendLine();
            sb.AppendLine("*Reviewed by `Arkn.MCP review_pattern` — [arkn docs](https://github.com/fernando-terra/arkn)*");
            return sb.ToString();
        }

        // ── Summary ───────────────────────────────────────────────────────────
        var errorCount   = violations.Count(v => GetRule(v) == "ARK004");
        var warningCount = violations.Count - errorCount;
        var score        = Math.Max(0, 100 - violations.Count * 8);
        var grade        = score switch
        {
            >= 90 => "A",
            >= 75 => "B",
            >= 60 => "C",
            >= 40 => "D",
            _     => "F",
        };

        sb.AppendLine($"**Score:** {score}/100 (Grade {grade})  |  " +
                      $"**Violations:** {violations.Count} " +
                      $"({errorCount} error{(errorCount != 1 ? "s" : "")}, " +
                      $"{warningCount} warning{(warningCount != 1 ? "s" : "")})");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // ── Violations, grouped by rule ───────────────────────────────────────
        var grouped = violations
            .GroupBy(GetRule)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var rule = group.Key;
            var (severity, description, fixHint) = _ruleInfo.TryGetValue(rule, out var info)
                ? info
                : ("⚠️ Warning", "Pattern violation", "See Arkn documentation.");

            sb.AppendLine($"## {severity} {rule} — {description}");
            sb.AppendLine();
            sb.AppendLine($"> **Fix:** {fixHint}");
            sb.AppendLine();

            foreach (var v in group)
            {
                var line    = GetLine(v);
                var message = GetMessage(v);
                var suggest = GetSuggestion(v);

                sb.AppendLine($"**Line {line}:** {message}");
                if (!string.IsNullOrWhiteSpace(suggest) && suggest != fixHint)
                    sb.AppendLine($"  → {suggest}");
                sb.AppendLine();
            }
        }

        // ── Recommendations ───────────────────────────────────────────────────
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Recommended next steps");
        sb.AppendLine();

        var rules = violations.Select(GetRule).Distinct().ToList();

        if (rules.Contains("ARK001") || rules.Contains("ARK007"))
            sb.AppendLine("1. **Eliminate exceptions from domain code** — every method that can fail should return `Result` or `Result<T>`. Use `scaffold_errors` to generate an `ErrorGroup` class.");
        if (rules.Contains("ARK002"))
            sb.AppendLine("2. **Fix error code naming** — codes must be `PascalCase.PascalCase`. Use `explain_error` to look up the correct code for your use case.");
        if (rules.Contains("ARK003"))
            sb.AppendLine("3. **Handle all Result values** — never discard a `Result`. Use `.Match()` or check `.IsSuccess` before proceeding.");
        if (rules.Contains("ARK005"))
            sb.AppendLine("4. **Replace raw HttpClient** — use `scaffold_http_client` to generate a typed `ArknHttpClient` with retry and timeout.");
        if (rules.Contains("ARK006"))
            sb.AppendLine("5. **Switch to IArknLogger** — structured, sink-routed logging across your entire application.");
        if (rules.Contains("ARK008"))
            sb.AppendLine("6. **Surface caught exceptions via Result.Failure** — use `migrate_exception` to convert catch blocks.");

        sb.AppendLine();
        sb.AppendLine("*Reviewed by `Arkn.MCP review_pattern` — [arkn docs](https://github.com/fernando-terra/arkn)*");

        return sb.ToString();
    }

    private static string GetRule(JsonElement v)       => TryGet(v, "Rule", "rule");
    private static int    GetLine(JsonElement v)       => v.TryGetProperty("Line", out var p) ? p.GetInt32()
                                                        : v.TryGetProperty("line", out var p2) ? p2.GetInt32() : 0;
    private static string GetMessage(JsonElement v)    => TryGet(v, "Message", "message");
    private static string GetSuggestion(JsonElement v) => TryGet(v, "Suggestion", "suggestion");

    private static string TryGet(JsonElement v, string key1, string key2)
        => v.TryGetProperty(key1, out var p) ? p.GetString() ?? ""
         : v.TryGetProperty(key2, out var p2) ? p2.GetString() ?? ""
         : "";
}
