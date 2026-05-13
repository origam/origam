#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Origam.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddParameterNameCodeFixProvider))]
[Shared]
public sealed class AddParameterNameCodeFixProvider : CodeFixProvider
{
    private const string FixEquivalenceKey = "Origam_AddArgumentName";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(ParameterNamingAnalyzer.AddNamesDiagnosticId);

    // BatchFixer applies each fix to an independent document copy and merges
    // the results, which corrupts argument lists that contain more than one
    // fixable literal. The custom provider rewrites every fixable argument in
    // a document in a single pass.
    public override FixAllProvider GetFixAllProvider() => new AddParameterNameFixAllProvider();

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context
            .Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var semanticModel = await context
            .Document.GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var argument = root.FindNode(diagnostic.Location.SourceSpan)
                .FirstAncestorOrSelf<ArgumentSyntax>();
            if (argument is null || argument.NameColon is not null)
            {
                continue;
            }

            // Only auto-fix literal arguments — non-literals (variables, method
            // calls, etc.) are flagged for context but left untouched.
            if (!IsReportableLiteral(argument.Expression))
            {
                continue;
            }

            var parameterName = ResolveParameterName(
                argument,
                semanticModel,
                context.CancellationToken
            );
            if (string.IsNullOrEmpty(parameterName))
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Add argument name '{parameterName}'",
                    createChangedDocument: ct =>
                        AddArgumentNameAsync(context.Document, argument, parameterName!, ct),
                    equivalenceKey: FixEquivalenceKey
                ),
                diagnostic
            );
        }
    }

    internal static NameColonSyntax CreateNameColon(string parameterName)
    {
        var safeName =
            SyntaxFacts.GetKeywordKind(parameterName) != SyntaxKind.None
                ? "@" + parameterName
                : parameterName;
        return SyntaxFactory.NameColon(safeName).WithTrailingTrivia(SyntaxFactory.Space);
    }

    internal static string? ResolveParameterName(
        ArgumentSyntax argument,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        if (argument.Parent is not ArgumentListSyntax argumentList || argumentList.Parent is null)
        {
            return null;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(argumentList.Parent, cancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var index = argumentList.Arguments.IndexOf(argument);
        if (index < 0 || index >= methodSymbol.Parameters.Length)
        {
            return null;
        }

        var parameter = methodSymbol.Parameters[index];

        // Skip params arguments — a named literal would need to be wrapped
        // in an explicit array, which is out of scope here.
        if (parameter.IsParams)
        {
            return null;
        }

        return parameter.Name;
    }

    internal static bool IsReportableLiteral(ExpressionSyntax expression)
    {
        if (
            expression is PrefixUnaryExpressionSyntax prefix
            && (
                prefix.IsKind(SyntaxKind.UnaryMinusExpression)
                || prefix.IsKind(SyntaxKind.UnaryPlusExpression)
            )
        )
        {
            expression = prefix.Operand;
        }

        return expression.Kind() switch
        {
            SyntaxKind.NullLiteralExpression => true,
            SyntaxKind.TrueLiteralExpression => true,
            SyntaxKind.FalseLiteralExpression => true,
            SyntaxKind.NumericLiteralExpression => true,
            SyntaxKind.StringLiteralExpression => true,
            SyntaxKind.CharacterLiteralExpression => true,
            SyntaxKind.DefaultLiteralExpression => true,
            _ => false,
        };
    }

    private static async Task<Document> AddArgumentNameAsync(
        Document document,
        ArgumentSyntax argument,
        string parameterName,
        CancellationToken cancellationToken
    )
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var newArgument = argument.WithNameColon(CreateNameColon(parameterName));
        var newRoot = root.ReplaceNode(argument, newArgument);
        return document.WithSyntaxRoot(newRoot);
    }
}

internal sealed class AddParameterNameFixAllProvider : FixAllProvider
{
    private const string Title = "Add argument names for literal arguments";
    private const string EquivalenceKey = "Origam_AddArgumentName";

    public override Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
    {
        CodeAction? action = fixAllContext.Scope switch
        {
            FixAllScope.Document when fixAllContext.Document is not null => CodeAction.Create(
                Title,
                ct => FixDocumentAsync(fixAllContext, fixAllContext.Document, ct),
                equivalenceKey: EquivalenceKey
            ),
            FixAllScope.Project => CodeAction.Create(
                Title,
                ct => FixProjectAsync(fixAllContext, fixAllContext.Project, ct),
                equivalenceKey: EquivalenceKey
            ),
            FixAllScope.Solution => CodeAction.Create(
                Title,
                ct => FixSolutionAsync(fixAllContext, ct),
                equivalenceKey: EquivalenceKey
            ),
            _ => null,
        };
        return Task.FromResult(action);
    }

    private static async Task<Solution> FixSolutionAsync(
        FixAllContext context,
        CancellationToken cancellationToken
    )
    {
        var solution = context.Solution;
        foreach (var project in solution.Projects)
        {
            solution = await FixProjectInternalAsync(context, project, solution, cancellationToken)
                .ConfigureAwait(false);
        }
        return solution;
    }

    private static async Task<Solution> FixProjectAsync(
        FixAllContext context,
        Project project,
        CancellationToken cancellationToken
    )
    {
        return await FixProjectInternalAsync(context, project, project.Solution, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<Solution> FixProjectInternalAsync(
        FixAllContext context,
        Project project,
        Solution solution,
        CancellationToken cancellationToken
    )
    {
        foreach (var document in project.Documents)
        {
            var fixedDocument = await FixDocumentAsync(context, document, cancellationToken)
                .ConfigureAwait(false);
            if (fixedDocument != document)
            {
                solution = solution.WithDocumentSyntaxRoot(
                    document.Id,
                    await fixedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
                        ?? throw new System.InvalidOperationException("Missing syntax root.")
                );
            }
        }
        return solution;
    }

    private static async Task<Document> FixDocumentAsync(
        FixAllContext context,
        Document document,
        CancellationToken cancellationToken
    )
    {
        var diagnostics = await context.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
        if (diagnostics.IsDefaultOrEmpty)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var semanticModel = await document
            .GetSemanticModelAsync(cancellationToken)
            .ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document;
        }

        var replacements = new Dictionary<ArgumentSyntax, ArgumentSyntax>();
        foreach (var diagnostic in diagnostics)
        {
            var argument = root.FindNode(diagnostic.Location.SourceSpan)
                .FirstAncestorOrSelf<ArgumentSyntax>();
            if (argument is null || argument.NameColon is not null)
            {
                continue;
            }

            if (replacements.ContainsKey(argument))
            {
                continue;
            }

            if (!AddParameterNameCodeFixProvider.IsReportableLiteral(argument.Expression))
            {
                continue;
            }

            var parameterName = AddParameterNameCodeFixProvider.ResolveParameterName(
                argument,
                semanticModel,
                cancellationToken
            );
            if (string.IsNullOrEmpty(parameterName))
            {
                continue;
            }

            replacements[argument] = argument.WithNameColon(
                AddParameterNameCodeFixProvider.CreateNameColon(parameterName!)
            );
        }

        if (replacements.Count == 0)
        {
            return document;
        }

        var newRoot = root.ReplaceNodes(replacements.Keys, (original, _) => replacements[original]);
        return document.WithSyntaxRoot(newRoot);
    }
}
