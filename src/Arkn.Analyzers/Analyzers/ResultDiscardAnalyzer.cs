using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Arkn.Analyzers.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arkn.Analyzers.Analyzers;

/// <summary>
/// ARK003 — Result or Result&lt;T&gt; values must not be silently discarded
/// (i.e. used as a standalone expression statement without assignment or return).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultDiscardAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.ARK003_ResultMustNotBeSilentlyDiscarded);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
    }

    private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        var statement = (ExpressionStatementSyntax)context.Node;

        // We only care about invocation expressions used as statements
        if (statement.Expression is not InvocationExpressionSyntax invocation) return;

        var typeInfo = context.SemanticModel.GetTypeInfo(invocation);
        var returnType = typeInfo.Type;

        if (returnType is null) return;
        if (!IsResultType(returnType)) return;

        // Determine a useful name for the diagnostic message
        string methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax m => m.Name.Identifier.Text,
            IdentifierNameSyntax id        => id.Identifier.Text,
            _                              => invocation.ToString()
        };

        var diagnostic = Diagnostic.Create(
            Descriptors.ARK003_ResultMustNotBeSilentlyDiscarded,
            statement.GetLocation(),
            methodName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsResultType(ITypeSymbol type)
    {
        // Direct: Result or Result<T>
        if (type.Name == "Result" && type.ContainingNamespace?.Name == "Results") return true;
        if (type.Name == "Result") return true; // broader check

        // Unwrap Task<Result> / ValueTask<Result>
        if (type is INamedTypeSymbol { TypeArguments.Length: 1 } generic &&
            (generic.Name is "Task" or "ValueTask"))
            return IsResultType(generic.TypeArguments[0]);

        return false;
    }
}
