﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class FormatInitializerOnSingleLineRefactoring
    {
        public static async Task<Document> RefactorAsync(
            Document document,
            InitializerExpressionSyntax initializer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InitializerExpressionSyntax newInitializer = initializer
                .WithExpressions(
                    SeparatedList(
                        initializer.Expressions.Select(expression => expression.WithoutTrivia())))
                .WithOpenBraceToken(OpenBraceToken())
                .WithCloseBraceToken(CloseBraceToken())
                .WithFormatterAnnotation();

            ExpressionSyntax newNode = GetNewExpression(newInitializer, (ExpressionSyntax)initializer.Parent);

            return await document.ReplaceNodeAsync(initializer.Parent, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static ExpressionSyntax GetNewExpression(InitializerExpressionSyntax initializer, ExpressionSyntax parent)
        {
            switch (parent.Kind())
            {
                case SyntaxKind.ObjectCreationExpression:
                    {
                        var expression = (ObjectCreationExpressionSyntax)parent;

                        ObjectCreationExpressionSyntax newNode = expression
                            .WithInitializer(initializer);

                        if (newNode.ArgumentList != null)
                        {
                            return newNode.WithArgumentList(newNode.ArgumentList.WithoutTrailingTrivia());
                        }
                        else
                        {
                            return newNode.WithType(newNode.Type.WithoutTrailingTrivia());
                        }
                    }
                case SyntaxKind.ArrayCreationExpression:
                    {
                        var expression = (ArrayCreationExpressionSyntax)parent;

                        return expression
                            .WithInitializer(initializer)
                            .WithType(expression.Type.WithoutTrailingTrivia());
                    }
                case SyntaxKind.ImplicitArrayCreationExpression:
                    {
                        var expression = (ImplicitArrayCreationExpressionSyntax)parent;

                        return expression
                            .WithInitializer(initializer)
                            .WithCloseBracketToken(expression.CloseBracketToken.WithoutTrailingTrivia());
                    }
            }

            return null;
        }
    }
}
