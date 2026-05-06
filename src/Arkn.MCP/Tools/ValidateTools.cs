using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Arkn.MCP.Tools;

[McpServerToolType]
public static class ValidateTools
{
    [McpServerTool, Description("Validates a C# code snippet against Arkn patterns (ARK001–ARK008) and returns violations with line numbers and fix suggestions.")]
    public static string ValidatePattern(
        [Description("C# source code to validate")] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "[]";

        var violations = new List<Violation>();
        var lines = code.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNum = i + 1;

            CheckArk001(line, lineNum, code, violations);
            CheckArk002(line, lineNum, violations);
            CheckArk003(line, lineNum, violations);
            CheckArk004(line, lineNum, violations);
            CheckArk005(line, lineNum, violations);
            CheckArk006(line, lineNum, violations);
            CheckArk007(line, lineNum, violations);
            CheckArk008(line, lineNum, violations);
        }

        return JsonSerializer.Serialize(violations, new JsonSerializerOptions { WriteIndented = true });
    }

    // ARK001 — public method returning void/Task (non-Result) in domain namespace
    private static void CheckArk001(string line, int lineNum, string fullCode, List<Violation> v)
    {
        var isDomainFile = Regex.IsMatch(fullCode, @"namespace\s+\w*\.?Domain", RegexOptions.IgnoreCase);
        if (!isDomainFile) return;

        if (Regex.IsMatch(line, @"\bpublic\b.*(void|Task)\s+\w+\s*\(") &&
            !Regex.IsMatch(line, @"Task<Result") &&
            !Regex.IsMatch(line, @"\boverride\b"))
        {
            v.Add(new("ARK001", lineNum,
                "Method in Domain namespace returns void or Task instead of Result.",
                "Return 'Result' or 'Task<Result>' to make failures explicit."));
        }
    }

    // ARK002 — Error code without Namespace.Reason (no dot or lowercase start)
    private static void CheckArk002(string line, int lineNum, List<Violation> v)
    {
        var m = Regex.Match(line, @"Error\.(NotFound|Validation|Conflict|Unauthorized|Forbidden|Failure)\s*\(\s*""([^""]+)""");
        if (!m.Success) return;

        var code = m.Groups[2].Value;
        if (!Regex.IsMatch(code, @"^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)+$"))
        {
            v.Add(new("ARK002", lineNum,
                $"Error code '{code}' does not follow 'Namespace.Reason' convention.",
                $"Use a dot-separated PascalCase code like '{char.ToUpper(code[0])}{code.Substring(1)}.Reason'."));
        }
    }

    // ARK003 — .Value accessed on Result without prior IsSuccess check or Match
    private static void CheckArk003(string line, int lineNum, List<Violation> v)
    {
        if (Regex.IsMatch(line, @"\bResult\b.*\.Value\b") &&
            !Regex.IsMatch(line, @"\bIsSuccess\b|\bMatch\b|\bif\b.*\bIsSuccess\b"))
        {
            v.Add(new("ARK003", lineNum,
                "Accessing .Value on Result without checking IsSuccess or using Match.",
                "Use .Match(onSuccess, onFailure) or check .IsSuccess before accessing .Value."));
        }
    }

    // ARK004 — IArknJob.ExecuteAsync with wrong return type
    private static void CheckArk004(string line, int lineNum, List<Violation> v)
    {
        if (Regex.IsMatch(line, @"\bExecuteAsync\b") &&
            Regex.IsMatch(line, @"\bTask\b") &&
            !Regex.IsMatch(line, @"Task<Result>"))
        {
            v.Add(new("ARK004", lineNum,
                "IArknJob.ExecuteAsync must return Task<Result>.",
                "Change the return type to 'Task<Result>'."));
        }
    }

    // ARK005 — new HttpClient() or directly injected HttpClient
    private static void CheckArk005(string line, int lineNum, List<Violation> v)
    {
        if (Regex.IsMatch(line, @"new\s+HttpClient\s*\(") ||
            Regex.IsMatch(line, @"\bHttpClient\b\s+\w+\s*[,\)]"))
        {
            v.Add(new("ARK005", lineNum,
                "HttpClient created directly or injected without Arkn.",
                "Use AddArknHttp<TClient>() for typed clients with retry and timeout built-in."));
        }
    }

    // ARK006 — Console.Write or MEL ILogger when IArknLogger is available
    private static void CheckArk006(string line, int lineNum, List<Violation> v)
    {
        if (Regex.IsMatch(line, @"\bConsole\.(Write|WriteLine)\b") ||
            (Regex.IsMatch(line, @"\bILogger\b(?!Factory)") && !Regex.IsMatch(line, @"IArknLogger")))
        {
            v.Add(new("ARK006", lineNum,
                "Using Console or MEL ILogger instead of IArknLogger.",
                "Inject IArknLogger and use its structured logging methods for consistency."));
        }
    }

    // ARK007 — throw new in domain method
    private static void CheckArk007(string line, int lineNum, List<Violation> v)
    {
        if (Regex.IsMatch(line, @"\bthrow\s+new\b"))
        {
            v.Add(new("ARK007", lineNum,
                "Exception thrown in domain code.",
                "Return Result.Failure(Error.*(code, message)) instead of throwing exceptions."));
        }
    }

    // ARK008 — empty catch or catch with only logging (swallowing errors)
    private static void CheckArk008(string line, int lineNum, List<Violation> v)
    {
        if (Regex.IsMatch(line, @"\bcatch\b\s*(\([^)]*\))?\s*\{?\s*(//.*)?$") &&
            !Regex.IsMatch(line, @"Result\.Failure"))
        {
            v.Add(new("ARK008", lineNum,
                "catch block appears to swallow the exception without returning Result.Failure.",
                "Return Result.Failure(Error.Failure(code, ex.Message)) to propagate the failure explicitly."));
        }
    }

    private sealed record Violation(string Rule, int Line, string Message, string Suggestion);
}
