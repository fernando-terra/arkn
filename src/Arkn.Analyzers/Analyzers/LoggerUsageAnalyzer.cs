using System.Linq;
using System.Collections.Immutable;
using Arkn.Analyzers.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arkn.Analyzers.Analyzers;

/// <summary>
/// ARK006 — Detects use of MEL ILogger or Console.Write in classes that implement IArknJob or extend ArknHttpClient.
/// These Arkn-managed components should use IArknLogger for consistent structured logging.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggerUsageAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.ARK006_PreferIArknLogger);

    /// <inheritdoc />
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

        // Only analyze Arkn-managed components
        if (!IsArknManagedComponent(classSymbol)) return;

        // Check constructor parameters for ILogger injection
        foreach (var ctor in classDecl.Members.OfType<ConstructorDeclarationSyntax>())
        {
            foreach (var param in ctor.ParameterList.Parameters)
            {
                if (IsMelLoggerType(context.SemanticModel, param.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.ARK006_PreferIArknLogger,
                        param.GetLocation(),
                        classSymbol.Name,
                        param.Type?.ToString() ?? "ILogger"));
                }
            }
        }

        // Check fields for ILogger
        foreach (var field in classDecl.Members.OfType<FieldDeclarationSyntax>())
        {
            if (IsMelLoggerType(context.SemanticModel, field.Declaration.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.ARK006_PreferIArknLogger,
                    field.GetLocation(),
                    classSymbol.Name,
                    field.Declaration.Type.ToString()));
            }
        }

        // Check Console.Write / Console.WriteLine invocations
        foreach (var invocation in classDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            if (memberAccess.Expression is not IdentifierNameSyntax id || id.Identifier.Text != "Console") continue;
            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName is not ("Write" or "WriteLine")) continue;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.ARK006_PreferIArknLogger,
                invocation.GetLocation(),
                classSymbol.Name,
                $"Console.{methodName}"));
        }
    }

    private static bool IsArknManagedComponent(INamedTypeSymbol type)
    {
        // Implements IArknJob
        if (type.AllInterfaces.Any(i => i.Name == "IArknJob")) return true;

        // Extends ArknHttpClient
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.Name == "ArknHttpClient") return true;
            current = current.BaseType;
        }
        return false;
    }

    private static bool IsMelLoggerType(SemanticModel model, TypeSyntax? typeSyntax)
    {
        if (typeSyntax is null) return false;
        var typeSymbol = model.GetTypeInfo(typeSyntax).Type;
        if (typeSymbol is null) return false;

        // Must be named ILogger (covers ILogger and ILogger<T> — generic arg doesn't change Name)
        if (typeSymbol.Name != "ILogger") return false;

        // Must be in a Microsoft.Extensions.Logging namespace
        var ns = typeSymbol.ContainingNamespace?.ToDisplayString() ?? "";
        return ns.Contains("Microsoft.Extensions.Logging") || ns.Contains("Logging");
    }
}
