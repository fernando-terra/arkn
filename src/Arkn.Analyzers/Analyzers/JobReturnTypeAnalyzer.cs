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
/// ARK004 — Classes implementing IArknJob must have ExecuteAsync returning Task&lt;Result&gt;.
/// Catches typos like Task&lt;bool&gt;, Task, or void that break the job runner contract.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class JobReturnTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.ARK004_JobExecuteAsyncMustReturnTaskResult);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDecl   = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol is null) return;

        // Check if class implements IArknJob
        if (!ImplementsIArknJob(classSymbol)) return;

        // Find ExecuteAsync method
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IMethodSymbol method) continue;
            if (method.Name != "ExecuteAsync") continue;

            // Return type must be Task<Result>
            if (IsTaskOfResult(method.ReturnType)) continue;

            // Find the corresponding syntax node for location
            var syntaxRef = method.DeclaringSyntaxReferences.FirstOrDefault();
            var location  = syntaxRef is not null
                ? syntaxRef.GetSyntax().GetLocation()
                : classDecl.Identifier.GetLocation();

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.ARK004_JobExecuteAsyncMustReturnTaskResult,
                location,
                method.Name));
        }
    }

    private static bool ImplementsIArknJob(INamedTypeSymbol type)
    {
        return type.AllInterfaces.Any(i => i.Name == "IArknJob");
    }

    private static bool IsTaskOfResult(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol { Name: "Task", TypeArguments.Length: 1 } task)
            return false;

        var inner = task.TypeArguments[0];
        return inner.Name == "Result";
    }
}
