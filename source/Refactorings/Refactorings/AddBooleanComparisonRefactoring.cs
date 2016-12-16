﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class AddBooleanComparisonRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, ExpressionSyntax expression)
        {
            ExpressionSyntax expression2 = GetExpression(expression);

            if (expression2 != null)
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                var namedTypeSymbol = semanticModel.GetConvertedTypeSymbol(expression2, context.CancellationToken) as INamedTypeSymbol;

                if (namedTypeSymbol?.IsNullableOf(SpecialType.System_Boolean) == true)
                    RegisterRefactoring(context, expression);
            }
        }

        public static void RegisterRefactoring(RefactoringContext context, ExpressionSyntax expression)
        {
            context.RegisterRefactoring(
                (expression.IsKind(SyntaxKind.LogicalNotExpression)) ? "Add ' == false'" : "Add ' == true'",
                cancellationToken => RefactorAsync(context.Document, expression, cancellationToken));
        }

        private static ExpressionSyntax GetExpression(ExpressionSyntax expression)
        {
            if (expression.IsKind(SyntaxKind.LogicalNotExpression))
            {
                var logicalNot = (PrefixUnaryExpressionSyntax)expression;

                if (logicalNot.Operand?.IsMissing == false)
                    return logicalNot.Operand;
            }
            else
            {
                return expression;
            }

            return null;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            BinaryExpressionSyntax newNode = CreateNewExpression(expression)
                .WithTriviaFrom(expression)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(expression, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static BinaryExpressionSyntax CreateNewExpression(ExpressionSyntax expression)
        {
            if (expression.IsKind(SyntaxKind.LogicalNotExpression))
            {
                var logicalNot = (PrefixUnaryExpressionSyntax)expression;

                return EqualsExpression(
                    logicalNot.Operand.WithoutTrivia(),
                    FalseLiteralExpression());
            }
            else
            {
                return EqualsExpression(
                    expression.WithoutTrivia(),
                    TrueLiteralExpression());
            }
        }
    }
}
