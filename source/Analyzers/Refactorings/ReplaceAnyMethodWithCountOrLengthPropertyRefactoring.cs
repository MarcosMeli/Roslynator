﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Extensions;
using Roslynator.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReplaceAnyMethodWithCountOrLengthPropertyRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess)
        {
            if (!invocation.IsParentKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(invocation, context.CancellationToken);

                if (methodSymbol != null
                    && Symbol.IsEnumerableMethodWithoutParameters(methodSymbol, "Any", context.SemanticModel))
                {
                    string propertyName = GetCountOrLengthPropertyName(memberAccess.Expression, context.SemanticModel, context.CancellationToken);

                    if (propertyName != null)
                    {
                        string messageArg = null;

                        TextSpan span = TextSpan.FromBounds(memberAccess.Name.Span.Start, invocation.Span.End);

                        if (invocation.DescendantTrivia(span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                        {
                            if (invocation.IsParentKind(SyntaxKind.LogicalNotExpression))
                            {
                                var logicalNot = (PrefixUnaryExpressionSyntax)invocation.Parent;

                                if (logicalNot.OperatorToken.TrailingTrivia.All(f => f.IsWhitespaceOrEndOfLineTrivia())
                                    && logicalNot.Operand.GetLeadingTrivia().All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                                {
                                    messageArg = $"{propertyName} == 0";
                                }
                            }
                            else
                            {
                                messageArg = $"{propertyName} > 0";
                            }
                        }

                        if (messageArg != null)
                        {
                            Diagnostic diagnostic = Diagnostic.Create(
                                DiagnosticDescriptors.ReplaceAnyMethodWithCountOrLengthProperty,
                                Location.Create(context.Node.SyntaxTree, span),
                                ImmutableDictionary.CreateRange(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("PropertyName", propertyName) }),
                                messageArg);

                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static string GetCountOrLengthPropertyName(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

            if (typeSymbol?.IsErrorType() == false
                && !typeSymbol.IsConstructedFromIEnumerableOfT())
            {
                if (typeSymbol.IsArrayType())
                    return "Length";

                if (Symbol.ImplementsICollectionOfT(typeSymbol))
                    return "Count";
            }

            return null;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            string propertyName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            memberAccess = memberAccess
                .WithName(IdentifierName(propertyName).WithTriviaFrom(memberAccess.Name));

            SyntaxNode newRoot = null;

            if (invocation.IsParentKind(SyntaxKind.LogicalNotExpression))
            {
                BinaryExpressionSyntax binaryExpression = BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    memberAccess,
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        Literal(0)));

                newRoot = root.ReplaceNode(
                    invocation.Parent,
                    binaryExpression.WithTriviaFrom(invocation.Parent));
            }
            else
            {
                BinaryExpressionSyntax binaryExpression = BinaryExpression(
                    SyntaxKind.GreaterThanExpression,
                    memberAccess,
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        Literal(0)));

                newRoot = root.ReplaceNode(
                    invocation,
                    binaryExpression.WithTriviaFrom(invocation));
            }

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
