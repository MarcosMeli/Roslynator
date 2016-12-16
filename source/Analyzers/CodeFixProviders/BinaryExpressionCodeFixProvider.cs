﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Refactorings.ReplaceCountMethod;

namespace Roslynator.CSharp.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BinaryExpressionCodeFixProvider))]
    [Shared]
    public class BinaryExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.SimplifyBooleanComparison,
                    DiagnosticIdentifiers.ReplaceCountMethodWithAnyMethod,
                    DiagnosticIdentifiers.AvoidNullLiteralExpressionOnLeftSideOfBinaryExpression,
                    DiagnosticIdentifiers.UseStringIsNullOrEmptyMethod);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            BinaryExpressionSyntax binaryExpression = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<BinaryExpressionSyntax>();

            if (binaryExpression == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.SimplifyBooleanComparison:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Simplify boolean comparison",
                                cancellationToken => SimplifyBooleanComparisonRefactoring.RefactorAsync(context.Document, binaryExpression, cancellationToken),
                                diagnostic.Id + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction, diagnostic);

                            break;
                        }
                    case DiagnosticIdentifiers.ReplaceCountMethodWithAnyMethod:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Replace 'Count()' with 'Any()'",
                                cancellationToken => ReplaceCountMethodWithAnyMethodRefactoring.RefactorAsync(context.Document, binaryExpression, cancellationToken),
                                diagnostic.Id + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.AvoidNullLiteralExpressionOnLeftSideOfBinaryExpression:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                $"Swap '{binaryExpression.Left}' and '{binaryExpression.Right}'",
                                cancellationToken => SwapExpressionsInBinaryExpressionRefactoring.RefactorAsync(context.Document, binaryExpression, cancellationToken),
                                diagnostic.Id + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseStringIsNullOrEmptyMethod:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use 'string.IsNullOrEmpty' method",
                                cancellationToken => UseStringIsNullOrEmptyMethodRefactoring.RefactorAsync(context.Document, binaryExpression, cancellationToken),
                                diagnostic.Id + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}