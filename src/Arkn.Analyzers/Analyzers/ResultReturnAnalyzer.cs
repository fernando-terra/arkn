using System.Linq;
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
/// ARK001 — Methods on Domain types (in namespaces containing "Domain") that could throw
/// should instead return Result or Result&lt;T&gt;.
/// Triggers on: public/internal methods in Domain types that declare 'throws' via XML doc
/// or whose body contains a throw statement returning void/non-Result types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultReturnAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.ARK001_DomainMethodShouldReturnResult);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(method);
        if (symbol is null) return;

        // Only analyze types in a namespace containing "Domain"
        if (!IsInDomainNamespace(symbol.ContainingNamespace)) return;

        // Only public or internal methods
        if (symbol.DeclaredAccessibility != Accessibility.Public &&
            symbol.DeclaredAccessibility != Accessibility.Internal) return;

        // Skip constructors, operators, property accessors
        if (symbol.MethodKind != MethodKind.Ordinary) return;

        // Already returns Result or Result<T>
        if (ReturnsResult(symbol.ReturnType)) return;

        // Check if method body contains a throw statement
        if (method.Body is null && method.ExpressionBody is null) return;

        var hasThrow = method.DescendantNodes()
            .OfType<ThrowStatementSyntax>()
            .Any();

        var hasThrowExpression = method.DescendantNodes()
            .OfType<ThrowExpressionSyntax>()
            .Any();

        if (!hasThrow && !hasThrowExpression) return;

        var diagnostic = Diagnostic.Create(
            Descriptors.ARK001_DomainMethodShouldReturnResult,
            method.Identifier.GetLocation(),
            symbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInDomainNamespace(INamespaceSymbol? ns)
    {
        while (ns is not null && !ns.IsGlobalNamespace)
        {
            if (ns.Name.Contains("Domain")) return true;
            ns = ns.ContainingNamespace;
        }
        return false;
    }

    private static bool ReturnsResult(ITypeSymbol returnType)
    {
        var name = returnType.Name;
        if (name == "Result") return true;

        // Handle Task<Result> and Task<Result<T>>
        if (returnType is INamedTypeSymbol { Name: "Task", TypeArguments.Length: 1 } taskType)
            return ReturnsResult(taskType.TypeArguments[0]);

        // Handle ValueTask<Result>
        if (returnType is INamedTypeSymbol { Name: "ValueTask", TypeArguments.Length: 1 } vtType)
            return ReturnsResult(vtType.TypeArguments[0]);

        return false;
    }
}
