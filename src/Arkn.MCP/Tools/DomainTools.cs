using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Arkn.MCP.Tools;

[McpServerToolType]
public static class DomainTools
{
    [McpServerTool, Description("Generates a domain Entity or AggregateRoot with optional Value Objects using Arkn.Core primitives and the Result pattern.")]
    public static string ScaffoldDomainEntity(
        [Description("Entity name in PascalCase, e.g. 'User', 'Order'")] string name,
        [Description("Comma-separated Value Object names, e.g. 'Email,Money,Address'. Leave empty for none.")] string valueObjects = "",
        [Description("True to generate an AggregateRoot (with domain events). False for a plain Entity.")] bool isAggregate = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Error: name cannot be empty.";

        var n   = char.ToUpperInvariant(name[0]) + name[1..];
        var vos = (valueObjects ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(v => char.ToUpperInvariant(v[0]) + v[1..])
                    .ToList();

        var baseClass = isAggregate ? "AggregateRoot" : "Entity";
        var sb        = new StringBuilder();

        sb.AppendLine("using Arkn.Core.Abstractions;");
        sb.AppendLine("using Arkn.Core.Primitives;");
        sb.AppendLine("using Arkn.Results;");
        sb.AppendLine();

        // Entity / AggregateRoot
        sb.AppendLine($"public sealed class {n} : {baseClass}");
        sb.AppendLine("{");

        // Properties
        foreach (var vo in vos)
            sb.AppendLine($"    public {vo} {vo} {{ get; private set; }} = null!;");

        if (vos.Count > 0) sb.AppendLine();

        // Private ctor for EF
        sb.AppendLine($"    private {n}() {{ }} // EF Core");
        sb.AppendLine();

        // Factory method
        var ctorParams = vos.Count > 0
            ? string.Join(", ", vos.Select(vo => $"string {char.ToLowerInvariant(vo[0])}{vo[1..]}"))
            : "/* add parameters */";

        sb.AppendLine($"    public static Result<{n}> Create({ctorParams})");
        sb.AppendLine("    {");

        foreach (var vo in vos)
        {
            var param = char.ToLowerInvariant(vo[0]) + vo[1..];
            sb.AppendLine($"        var {param}Vo = {vo}.Create({param});");
            sb.AppendLine($"        if ({param}Vo.IsFailure) return {param}Vo.Error;");
            sb.AppendLine();
        }

        sb.AppendLine($"        var entity = new {n}");
        sb.AppendLine("        {");
        foreach (var vo in vos)
        {
            var param = char.ToLowerInvariant(vo[0]) + vo[1..];
            sb.AppendLine($"            {vo} = {param}Vo.Value,");
        }
        sb.AppendLine("        };");
        sb.AppendLine();

        if (isAggregate)
            sb.AppendLine($"        entity.Raise(new {n}CreatedEvent(entity.Id));");

        sb.AppendLine($"        return entity;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Domain event if aggregate
        if (isAggregate)
        {
            sb.AppendLine($"public sealed record {n}CreatedEvent(Guid {n}Id) : IDomainEvent");
            sb.AppendLine("{");
            sb.AppendLine("    public DateTime OccurredOn { get; } = DateTime.UtcNow;");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Value Objects
        foreach (var vo in vos)
        {
            var param = char.ToLowerInvariant(vo[0]) + vo[1..];
            sb.AppendLine($"public sealed class {vo} : ValueObject");
            sb.AppendLine("{");
            sb.AppendLine($"    public string Value {{ get; }}");
            sb.AppendLine();
            sb.AppendLine($"    private {vo}(string value) => Value = value;");
            sb.AppendLine();
            sb.AppendLine($"    public static Result<{vo}> Create(string value)");
            sb.AppendLine("    {");
            sb.AppendLine($"        if (string.IsNullOrWhiteSpace(value))");
            sb.AppendLine($"            return Error.Validation(\"{n}Errors.{vo}Required\", \"{vo} is required.\");");
            sb.AppendLine();
            sb.AppendLine($"        // TODO: add domain-specific validation");
            sb.AppendLine();
            sb.AppendLine($"        return new {vo}(value);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    protected override IEnumerable<object?> GetEqualityComponents()");
            sb.AppendLine("    {");
            sb.AppendLine("        yield return Value;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
