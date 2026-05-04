using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Composition;
using Arkn.Analyzers.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Arkn.Analyzers.CodeFixes;

/// <summary>
/// Code fix for ARK001: offers to wrap the return type in Result&lt;T&gt;.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultReturnCodeFix)), Shared]
public sealed class ResultReturnCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Descriptors.ARK001_DomainMethodShouldReturnResult.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is null) return;

        var diagnostic    = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var methodDecl = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (methodDecl is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Wrap return type in Result<T>",
                createChangedDocument: ct => WrapReturnTypeAsync(context.Document, methodDecl, ct),
                equivalenceKey: "WrapInResult"),
            diagnostic);
    }

    private static async Task<Document> WrapReturnTypeAsync(
        Document document,
        MethodDeclarationSyntax method,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct);
        if (root is null) return document;

        var currentReturn = method.ReturnType;
        var resultType    = SyntaxFactory.GenericName(
            SyntaxFactory.Identifier("Result"),
            SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SingletonSeparatedList(currentReturn.WithoutTrivia())))
            .WithTriviaFrom(currentReturn);

        var newMethod = method.WithReturnType(resultType);
        var newRoot   = root.ReplaceNode(method, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }
}
