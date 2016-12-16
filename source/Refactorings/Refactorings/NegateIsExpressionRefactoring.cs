﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class NegateIsExpressionRefactoring
    {
        private const string Title = "Negate is";

        public static void ComputeRefactoring(RefactoringContext context, BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.IsExpression)
                && context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(binaryExpression))
            {
                SyntaxNode parent = binaryExpression.Parent;

                if (parent?.IsKind(SyntaxKind.ParenthesizedExpression) == true)
                {
                    if (parent.Parent?.IsKind(SyntaxKind.LogicalNotExpression) == true)
                    {
                        RegisterRefactoring(context, (ExpressionSyntax)parent.Parent);
                    }
                    else
                    {
                        RegisterRefactoring(context, (ExpressionSyntax)parent);
                    }
                }
                else
                {
                    RegisterRefactoring(context, binaryExpression);
                }
            }
        }

        private static void RegisterRefactoring(RefactoringContext context, ExpressionSyntax expression)
        {
            context.RegisterRefactoring(
                Title,
                cancellationToken => RefactorAsync(context.Document, expression, context.CancellationToken));
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            return await document.ReplaceNodeAsync(expression, expression.LogicallyNegate(), cancellationToken).ConfigureAwait(false);
        }
    }
}