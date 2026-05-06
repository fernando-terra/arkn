using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Arkn.MCP.Tools;

[McpServerToolType]
public static class AnalysisTools
{
    // ── project_health ────────────────────────────────────────────────────────

    [McpServerTool, Description("Analyzes multiple C# source files against Arkn patterns (ARK001–ARK008) and returns an aggregate health report with a score, violation counts and top issues.")]
    public static string ProjectHealth(
        [Description("Array of C# source file contents to analyze")] string[] files)
    {
        if (files is null || files.Length == 0)
            return JsonSerializer.Serialize(new { score = 100, totalViolations = 0, byRule = new { }, topIssues = Array.Empty<string>(), verdict = "Excellent — no files provided" });

        var byRule = new Dictionary<string, int>
        {
            ["ARK001"] = 0, ["ARK002"] = 0, ["ARK003"] = 0, ["ARK004"] = 0,
            ["ARK005"] = 0, ["ARK006"] = 0, ["ARK007"] = 0, ["ARK008"] = 0,
        };

        foreach (var fileContent in files)
        {
            if (string.IsNullOrWhiteSpace(fileContent)) continue;
            var json  = ValidateTools.ValidatePattern(fileContent);
            var items = JsonSerializer.Deserialize<JsonElement[]>(json) ?? [];
            foreach (var item in items)
            {
                var rule = item.GetProperty("rule").GetString() ?? "";
                if (byRule.ContainsKey(rule)) byRule[rule]++;
            }
        }

        var total   = byRule.Values.Sum();
        var score   = Math.Max(0, 100 - total * 5);
        var verdict = score switch
        {
            >= 90 => "Excellent — code is well-aligned with Arkn patterns",
            >= 70 => "Good — minor issues to fix",
            >= 50 => "Fair — several pattern violations detected",
            _     => "Needs Work — significant refactoring recommended",
        };

        var topIssues = byRule
            .Where(kv => kv.Value > 0)
            .OrderByDescending(kv => kv.Value)
            .Take(5)
            .Select(kv => RuleDescription(kv.Key, kv.Value))
            .ToArray();

        return JsonSerializer.Serialize(new
        {
            score,
            totalViolations = total,
            byRule,
            topIssues,
            verdict,
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string RuleDescription(string rule, int count) => rule switch
    {
        "ARK001" => $"ARK001: {count} domain method(s) returning void/Task instead of Result",
        "ARK002" => $"ARK002: {count} error code(s) missing Namespace.Reason convention",
        "ARK003" => $"ARK003: {count} Result(s) silently discarded",
        "ARK004" => $"ARK004: {count} ExecuteAsync method(s) not returning Task<Result>",
        "ARK005" => $"ARK005: {count} raw HttpClient usage(s)",
        "ARK006" => $"ARK006: {count} MEL ILogger usage(s) instead of IArknLogger",
        "ARK007" => $"ARK007: {count} throw statement(s) in domain code",
        "ARK008" => $"ARK008: {count} catch block(s) swallowing exceptions",
        _        => $"{rule}: {count} violation(s)",
    };

    // ── list_arkn_types ───────────────────────────────────────────────────────

    [McpServerTool, Description("Scans C# source files and returns an inventory of Arkn types: error groups, jobs (with cron), HTTP clients, and error codes per group.")]
    public static string ListArknTypes(
        [Description("Array of C# source file contents to scan")] string[] files)
    {
        var errorGroups = new List<string>();
        var jobs        = new List<object>();
        var httpClients = new List<string>();
        var errorCodes  = new Dictionary<string, List<string>>();

        foreach (var content in files ?? [])
        {
            if (string.IsNullOrWhiteSpace(content)) continue;

            // [ArknErrors] classes
            foreach (Match m in Regex.Matches(content, @"\[ArknErrors\][\s\S]*?class\s+(\w+)"))
            {
                var name = m.Groups[1].Value;
                if (!errorGroups.Contains(name)) errorGroups.Add(name);

                // Error codes inside that group
                var codes = new List<string>();
                var block = ExtractClassBlock(content, name);
                foreach (Match cm in Regex.Matches(block, @"\[ArknErrorCode[^\]]*\].*?(?:partial\s+Error|static\s+partial\s+Error)\s+(\w+)"))
                    codes.Add(cm.Groups[1].Value);
                if (codes.Count > 0) errorCodes[name] = codes;
            }

            // IArknJob implementations
            foreach (Match m in Regex.Matches(content, @"class\s+(\w+)\s*[:\(][^{]*IArknJob"))
            {
                var name = m.Groups[1].Value;
                // Try to find associated cron via Add<ClassName> or comment
                var cronMatch = Regex.Match(content, $@"Add<{name}>\s*\(\s*""([^""]+)""");
                var cron      = cronMatch.Success ? cronMatch.Groups[1].Value : "";
                if (jobs.All(j => ((dynamic)j).name != name))
                    jobs.Add(new { name, cron });
            }

            // ArknHttpClient subclasses
            foreach (Match m in Regex.Matches(content, @"class\s+(\w+)[^{]*:\s*ArknHttpClient"))
            {
                var name = m.Groups[1].Value;
                if (!httpClients.Contains(name)) httpClients.Add(name);
            }
        }

        return JsonSerializer.Serialize(new
        {
            errorGroups,
            jobs,
            httpClients,
            errorCodes,
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string ExtractClassBlock(string content, string className)
    {
        var idx = content.IndexOf($"class {className}", StringComparison.Ordinal);
        if (idx < 0) return string.Empty;
        var start = content.IndexOf('{', idx);
        if (start < 0) return string.Empty;
        int depth = 1, pos = start + 1;
        while (pos < content.Length && depth > 0)
        {
            if (content[pos] == '{') depth++;
            else if (content[pos] == '}') depth--;
            pos++;
        }
        return content.Substring(start, pos - start);
    }

    // ── scaffold_minimal_api ──────────────────────────────────────────────────

    [McpServerTool, Description("Generates a Minimal API endpoint group for a resource with Result→HTTP mapping for each requested operation (get, create, update, delete).")]
    public static string ScaffoldMinimalApi(
        [Description("Resource name in PascalCase, e.g. 'Payment', 'User'")] string resource,
        [Description("Comma-separated operations: get, getall, create, update, delete")] string operations,
        [Description("Error group class name, e.g. 'PaymentErrors'. Leave empty to use generic error mapping.")] string errorGroup = "")
    {
        if (string.IsNullOrWhiteSpace(resource))
            return "Error: resource cannot be empty.";

        var r      = char.ToUpperInvariant(resource[0]) + resource[1..];
        var rLower = char.ToLowerInvariant(resource[0]) + resource[1..] + "s";
        var ops    = (operations ?? "get,create,update,delete")
                       .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .Select(o => o.ToLowerInvariant())
                       .ToHashSet();

        var sb = new StringBuilder();
        sb.AppendLine($"// ── {r} Endpoints — generated by Arkn.MCP scaffold_minimal_api ──────────");
        sb.AppendLine("using Arkn.Results;");
        sb.AppendLine();
        sb.AppendLine($"var {rLower}Group = app.MapGroup(\"/{rLower}\").WithTags(\"{r}\");");
        sb.AppendLine();

        if (ops.Contains("getall"))
        {
            sb.AppendLine($"// GET /{rLower}");
            sb.AppendLine($"{rLower}Group.MapGet(\"/\", async (I{r}Service svc) =>");
            sb.AppendLine("{");
            sb.AppendLine($"    var result = await svc.GetAllAsync();");
            sb.AppendLine(MapResult(r));
            sb.AppendLine("});");
            sb.AppendLine();
        }

        if (ops.Contains("get"))
        {
            sb.AppendLine($"// GET /{rLower}/{{id}}");
            sb.AppendLine($"{rLower}Group.MapGet(\"{{id:guid}}\", async (Guid id, I{r}Service svc) =>");
            sb.AppendLine("{");
            sb.AppendLine($"    var result = await svc.GetByIdAsync(id);");
            sb.AppendLine(MapResult(r));
            sb.AppendLine("});");
            sb.AppendLine();
        }

        if (ops.Contains("create"))
        {
            sb.AppendLine($"// POST /{rLower}");
            sb.AppendLine($"{rLower}Group.MapPost(\"/\", async (Create{r}Request request, I{r}Service svc) =>");
            sb.AppendLine("{");
            sb.AppendLine($"    var result = await svc.CreateAsync(request);");
            sb.AppendLine(MapResult(r, isCreate: true));
            sb.AppendLine("});");
            sb.AppendLine();
        }

        if (ops.Contains("update"))
        {
            sb.AppendLine($"// PUT /{rLower}/{{id}}");
            sb.AppendLine($"{rLower}Group.MapPut(\"{{id:guid}}\", async (Guid id, Update{r}Request request, I{r}Service svc) =>");
            sb.AppendLine("{");
            sb.AppendLine($"    var result = await svc.UpdateAsync(id, request);");
            sb.AppendLine(MapResult(r));
            sb.AppendLine("});");
            sb.AppendLine();
        }

        if (ops.Contains("delete"))
        {
            sb.AppendLine($"// DELETE /{rLower}/{{id}}");
            sb.AppendLine($"{rLower}Group.MapDelete(\"{{id:guid}}\", async (Guid id, I{r}Service svc) =>");
            sb.AppendLine("{");
            sb.AppendLine($"    var result = await svc.DeleteAsync(id);");
            sb.AppendLine(MapResultNoValue());
            sb.AppendLine("});");
        }

        return sb.ToString();
    }

    private static string MapResult(string resource, bool isCreate = false)
    {
        var ok = isCreate
            ? $"dto => Results.Created($\"/{resource.ToLowerInvariant()}s/{{dto.Id}}\", dto)"
            : "dto => Results.Ok(dto)";

        return $"""
                return result.Match(
                    onSuccess: {ok},
                    onFailure: error => error.Type switch
                    {{
                        ErrorType.NotFound   => Results.NotFound(new {{ error.Code, error.Message }}),
                        ErrorType.Validation => Results.BadRequest(new {{ error.Code, error.Message }}),
                        ErrorType.Conflict   => Results.Conflict(new {{ error.Code, error.Message }}),
                        ErrorType.Unauthorized => Results.Unauthorized(),
                        _                    => Results.Problem(error.Message)
                    }});
        """;
    }

    private static string MapResultNoValue() => """
                return result.Match(
                    onSuccess: () => Results.NoContent(),
                    onFailure: error => error.Type switch
                    {
                        ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
                        _                  => Results.Problem(error.Message)
                    });
        """;
}
