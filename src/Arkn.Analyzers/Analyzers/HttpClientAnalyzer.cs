using System.Linq;
using System.Collections.Immutable;
using Arkn.Analyzers.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arkn.Analyzers.Analyzers;

/// <summary>
/// ARK005 — Detects direct use of HttpClient in classes that are not ArknHttpClient subclasses.
/// Triggers on: fields, constructor parameters, and object creation expressions of type HttpClient.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HttpClientAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.ARK005_AvoidRawHttpClient);

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

        // Skip if the class IS ArknHttpClient or inherits from it
        if (InheritsArknHttpClient(classSymbol)) return;

        // Check constructor parameters
        foreach (var ctor in classDecl.Members.OfType<ConstructorDeclarationSyntax>())
        {
            foreach (var param in ctor.ParameterList.Parameters)
            {
                if (IsHttpClientType(context.SemanticModel, param.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.ARK005_AvoidRawHttpClient,
                        param.GetLocation(),
                        classSymbol.Name));
                }
            }
        }

        // Check fields
        foreach (var field in classDecl.Members.OfType<FieldDeclarationSyntax>())
        {
            if (IsHttpClientType(context.SemanticModel, field.Declaration.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.ARK005_AvoidRawHttpClient,
                    field.GetLocation(),
                    classSymbol.Name));
            }
        }

        // Check object creation: new HttpClient()
        foreach (var creation in classDecl.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            var typeSymbol = context.SemanticModel.GetTypeInfo(creation).Type;
            if (typeSymbol?.Name == "HttpClient" &&
                typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Net.Http")
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.ARK005_AvoidRawHttpClient,
                    creation.GetLocation(),
                    classSymbol.Name));
            }
        }
    }

    private static bool IsHttpClientType(SemanticModel model, TypeSyntax? typeSyntax)
    {
        if (typeSyntax is null) return false;
        var typeSymbol = model.GetTypeInfo(typeSyntax).Type;
        return typeSymbol?.Name == "HttpClient"
            && typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Net.Http";
    }

    private static bool InheritsArknHttpClient(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.Name == "ArknHttpClient") return true;
            current = current.BaseType;
        }
        return false;
    }
}
