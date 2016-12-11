﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class IfStatementRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, IfStatementSyntax ifStatement)
        {
            if (context.IsAnyRefactoringEnabled(
                    RefactoringIdentifiers.SwapStatementsInIfElse,
                    RefactoringIdentifiers.ReplaceIfElseWithConditionalExpression,
                    RefactoringIdentifiers.ReplaceIfStatementWithReturnStatement,
                    RefactoringIdentifiers.SplitIfElse)
                && IfElseChain.IsTopmostIf(ifStatement)
                && context.Span.IsBetweenSpans(ifStatement))
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceIfStatementWithReturnStatement))
                    ReplaceIfStatementWithReturnStatementRefactoring.ComputeRefactoring(context, ifStatement);

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceIfElseWithConditionalExpression))
                    ReplaceIfElseWithConditionalExpressionRefactoring.ComputeRefactoring(context, ifStatement);

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.SwapStatementsInIfElse))
                    SwapStatementInIfElseRefactoring.ComputeRefactoring(context, ifStatement);

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.SplitIfElse))
                    SplitIfElseRefactoring.ComputeRefactoring(context, ifStatement);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.AddBooleanComparison)
                && ifStatement.Condition != null
                && ifStatement.Condition.Span.Contains(context.Span))
            {
                await AddBooleanComparisonRefactoring.ComputeRefactoringAsync(context, ifStatement.Condition).ConfigureAwait(false);
            }
        }
    }
}