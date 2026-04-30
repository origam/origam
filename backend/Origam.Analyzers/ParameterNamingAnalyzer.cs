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

        var parameterCount = methodSymbol.Parameters.Length;
        if (parameterCount <= 1)
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
