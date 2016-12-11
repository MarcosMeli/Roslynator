// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class SplitIfElseRefactoring
    {
        public static void ComputeRefactoring(RefactoringContext context, IfStatementSyntax ifStatement)
        {
            if (IfElseChain.IsTopmostIf(ifStatement)
                && IfElseChain.ContainsElseIf(ifStatement))
            {
                context.RegisterRefactoring(
                    "Split if-else",
                    cancellationToken => RefactorAsync(context.Document, ifStatement, cancellationToken));
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            IEnumerable<StatementSyntax> newNodes = SplitIfStatements(ifStatement)
                .Select(f => f.WithFormatterAnnotation());

            return await document.ReplaceNodeAsync(ifStatement, newNodes, cancellationToken).ConfigureAwait(false);
        }

        private static IEnumerable<StatementSyntax> SplitIfStatements(IfStatementSyntax ifStatement)
        {
            yield return ifStatement.WithElse(null);

            ElseClauseSyntax elseClause = ifStatement.Else;
            StatementSyntax statement = elseClause.Statement;

            ifStatement = (IfStatementSyntax)statement;

            while (true)
            {
                yield return ifStatement.WithElse(null).SetLeadingTrivia(elseClause);

                elseClause = ifStatement.Else;
                statement = elseClause?.Statement;

                if (statement != null)
                {
                    if (statement.IsKind(SyntaxKind.IfStatement))
                    {
                        ifStatement = (IfStatementSyntax)statement;
                    }
                    else
                    {
                        if (statement.IsKind(SyntaxKind.Block))
                        {
                            foreach (StatementSyntax statement2 in ((BlockSyntax)statement).Statements)
                                yield return statement2.SetLeadingTrivia(elseClause);
                        }
                        else
                        {
                            yield return statement.SetLeadingTrivia(elseClause);
                        }

                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private static StatementSyntax SetLeadingTrivia(this StatementSyntax statement, ElseClauseSyntax elseClause)
        {
            SyntaxTriviaList leadingTrivia = elseClause.GetLeadingTrivia();

            var list = new List<SyntaxTrivia>(leadingTrivia.Count + 1);
            list.Add(NewLineTrivia());
            list.AddRange(leadingTrivia);

            return statement.WithLeadingTrivia(list);
        }
    }
}