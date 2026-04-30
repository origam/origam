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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Origam.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterNamingAnalyzer : DiagnosticAnalyzer
{
    public const string AddNamesDiagnosticId = "ORIGAM002";

    private static readonly DiagnosticDescriptor AddNamesRule = new(
        AddNamesDiagnosticId,
        "Use parameter names for multi-parameter methods",
        "Add parameter names for multi-parameter method",
        "Style",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(AddNamesRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(
            AnalyzeObjectCreation,
            SyntaxKind.ObjectCreationExpression
        );
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var methodSymbol = GetMethodSymbol(context, invocation);
        AnalyzeArgumentList(context, invocation.ArgumentList, methodSymbol);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
        var methodSymbol = GetMethodSymbol(context, objectCreation);
        AnalyzeArgumentList(context, objectCreation.ArgumentList, methodSymbol);
    }

    private static IMethodSymbol? GetMethodSymbol(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node
    )
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(node, context.CancellationToken);
        return symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
    }

    private static void AnalyzeArgumentList(
        SyntaxNodeAnalysisContext context,
        ArgumentListSyntax? argumentList,
        IMethodSymbol? methodSymbol
    )
    {
        if (argumentList is null || methodSymbol is null)
        {
            return;
        }

        var argumentCount = argumentList.Arguments.Count;
        if (argumentCount <= 1)
        {
            return;
        }

        foreach (var argument in argumentList.Arguments)
        {
            if (argument.NameColon is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(AddNamesRule, argument.GetLocation()));
            }
        }
    }
}
