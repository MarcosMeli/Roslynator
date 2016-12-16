﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Immutable;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReplaceMethodInvocationWithElementAccessRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression?.IsKind(SyntaxKind.SimpleMemberAccessExpression) == true
                && invocation.ArgumentList != null)
            {
                var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

                string methodName = memberAccess.Name?.Identifier.ValueText;

                switch (methodName)
                {
                    case "First":
                    case "Last":
                        {
                            await ProcessFirstOrLastAsync(context, invocation, methodName).ConfigureAwait(false);
                            break;
                        }
                    case "ElementAt":
                        {
                            await ProcessElementAtAsync(context, invocation).ConfigureAwait(false);
                            break;
                        }
                }
            }
        }

        private static async Task ProcessFirstOrLastAsync(RefactoringContext context, InvocationExpressionSyntax invocation, string methodName)
        {
            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            if (invocation.ArgumentList.Arguments.Count == 0
                && SemanticAnalyzer.IsEnumerableExtensionOrImmutableArrayExtensionMethod(invocation, methodName, 1, semanticModel))
            {
                var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

                ITypeSymbol typeSymbol = semanticModel
                    .GetTypeInfo(memberAccess.Expression, context.CancellationToken)
                    .Type;

                if (typeSymbol != null
                    && (typeSymbol.IsArrayType() || typeSymbol.HasPublicIndexer()))
                {
                    string propertyName = GetCountOrLengthPropertyName(memberAccess.Expression, semanticModel, context.CancellationToken);

                    if (propertyName != null)
                    {
                        context.RegisterRefactoring(
                            $"Replace '{methodName}' with '[]'",
                            cancellationToken =>
                            {
                                return RefactorAsync(
                                    context.Document,
                                    invocation,
                                    propertyName,
                                    context.CancellationToken);
                            });
                    }
                }
            }
        }

        private static async Task ProcessElementAtAsync(RefactoringContext context, InvocationExpressionSyntax invocation)
        {
            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            if (invocation.ArgumentList?.Arguments.Count == 1
                && (SemanticAnalyzer.IsEnumerableElementAtMethod(invocation, semanticModel)
                    || SemanticAnalyzer.IsImmutableArrayElementAtMethod(invocation, semanticModel)))
            {
                var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

                ITypeSymbol typeSymbol = semanticModel
                    .GetTypeInfo(memberAccess.Expression, context.CancellationToken)
                    .Type;

                if (typeSymbol != null
                    && (typeSymbol.IsArrayType() || typeSymbol.HasPublicIndexer()))
                {
                    context.RegisterRefactoring(
                        "Replace 'ElementAt' with '[]'",
                        cancellationToken => RefactorAsync(context.Document, invocation, null, cancellationToken));
                }
            }
        }

        private static string GetCountOrLengthPropertyName(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ITypeSymbol typeSymbol = semanticModel
                .GetTypeInfo(expression, cancellationToken)
                .Type;

            if (typeSymbol?.IsErrorType() == false
                && !typeSymbol.IsConstructedFromIEnumerableOfT())
            {
                if (typeSymbol.BaseType?.SpecialType == SpecialType.System_Array)
                    return "Length";

                if (typeSymbol.IsConstructedFromImmutableArrayOfT(semanticModel))
                    return "Length";

                ImmutableArray<INamedTypeSymbol> allInterfaces = typeSymbol.AllInterfaces;

                for (int i = 0; i < allInterfaces.Length; i++)
                {
                    if (allInterfaces[i].ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_ICollection_T)
                    {
                        foreach (ISymbol members in typeSymbol.GetMembers("Count"))
                        {
                            if (members.IsProperty()
                                && members.IsPublic())
                            {
                                return "Count";
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            string propertyName = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            ElementAccessExpressionSyntax elementAccess = ElementAccessExpression(
                memberAccess.Expression.WithoutTrailingTrivia(),
                BracketedArgumentList(
                    SingletonSeparatedList(
                        Argument(CreateArgumentExpression(invocation, memberAccess, propertyName)))));

            return await document.ReplaceNodeAsync(
                invocation,
                elementAccess.WithTriviaFrom(invocation),
                cancellationToken).ConfigureAwait(false);
        }

        private static ExpressionSyntax CreateArgumentExpression(
            InvocationExpressionSyntax invocation,
            MemberAccessExpressionSyntax memberAccess,
            string propertyName)
        {
            switch (memberAccess.Name.Identifier.ValueText)
            {
                case "First":
                    {
                        return NumericLiteralExpression(0);
                    }
                case "Last":
                    {
                        return SubtractExpression(
                            SimpleMemberAccessExpression(
                                ProcessExpression(memberAccess.Expression),
                                IdentifierName(propertyName)),
                            NumericLiteralExpression(1));
                    }
                case "ElementAt":
                    {
                        return ProcessExpression(invocation.ArgumentList.Arguments[0].Expression);
                    }
            }

            return default(ExpressionSyntax);
        }

        private static ExpressionSyntax ProcessExpression(ExpressionSyntax expression)
        {
            if (expression
                .DescendantTrivia(expression.Span)
                .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
            {
                expression = SyntaxRemover.RemoveWhitespaceOrEndOfLine(expression)
                    .WithFormatterAnnotation();
            }

            return expression;
        }
    }
}
