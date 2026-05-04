using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Arkn.Analyzers.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arkn.Analyzers.Analyzers;

/// <summary>
/// ARK002 — Error code strings passed to Error.NotFound(), Error.Validation(), etc.
/// must follow the 'Namespace.Reason' pattern (dot-separated PascalCase segments).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorCodeNamingAnalyzer : DiagnosticAnalyzer
{
    // Matches: PascalCase.PascalCase (at least two segments, each starting with uppercase)
    private static readonly Regex ValidPattern =
        new(@"^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)+$", RegexOptions.Compiled);

    private static readonly HashSet<string> ErrorFactoryMethods = new(StringComparer.Ordinal)
    {
        "Failure", "NotFound", "Validation", "Conflict", "Unauthorized", "Forbidden"
    };

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.ARK002_ErrorCodeMustFollowNamingConvention);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Must be a member access: Error.Xxx(...)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return;
        if (memberAccess.Expression is not IdentifierNameSyntax typeName) return;
        if (typeName.Identifier.Text != "Error") return;
        if (!ErrorFactoryMethods.Contains(memberAccess.Name.Identifier.Text)) return;

        // First argument must be a string literal (the error code)
        var args = invocation.ArgumentList.Arguments;
        if (args.Count == 0) return;

        var firstArg = args[0].Expression;
        if (firstArg is not LiteralExpressionSyntax literal) return;
        if (!literal.IsKind(SyntaxKind.StringLiteralExpression)) return;

        var code = literal.Token.ValueText;
        if (ValidPattern.IsMatch(code)) return;

        var diagnostic = Diagnostic.Create(
            Descriptors.ARK002_ErrorCodeMustFollowNamingConvention,
            literal.GetLocation(),
            code);

        context.ReportDiagnostic(diagnostic);
    }
}
