﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Refactorings
{
    internal static class MergeLocalDeclarationWithReturnStatementRefactoring
    {
        private static DiagnosticDescriptor FadeOutDescriptor
        {
            get { return DiagnosticDescriptors.MergeLocalDeclarationWithReturnStatementFadeOut; }
        }

        public static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;
            ExpressionSyntax expression = returnStatement.Expression;

            if (expression?.IsKind(SyntaxKind.IdentifierName) != true)
                return;

            LocalDeclarationStatementSyntax localDeclaration = GetLocalDeclaration(returnStatement);

            if (localDeclaration == null)
                return;

            VariableDeclaratorSyntax declarator = GetVariableDeclarator(localDeclaration);

            if (declarator == null)
                return;

            ISymbol symbol = context.SemanticModel.GetSymbol(expression, context.CancellationToken);

            if (symbol?.IsLocal() != true)
                return;

            ISymbol symbol2 = context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken);

            if (symbol.Equals(symbol2))
            {
                TextSpan span = TextSpan.FromBounds(localDeclaration.Span.Start, returnStatement.Span.End);

                if (returnStatement.Parent
                    .DescendantTrivia(span, descendIntoTrivia: false)
                    .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.MergeLocalDeclarationWithReturnStatement,
                        Location.Create(context.Node.SyntaxTree, span));
                }

                context.FadeOutNode(FadeOutDescriptor, localDeclaration.Declaration.Type);
                context.FadeOutToken(FadeOutDescriptor, declarator.Identifier);
                context.FadeOutToken(FadeOutDescriptor, declarator.Initializer.EqualsToken);
                context.FadeOutToken(FadeOutDescriptor, localDeclaration.SemicolonToken);
                context.FadeOutNode(FadeOutDescriptor, expression);
            }
        }

        private static LocalDeclarationStatementSyntax GetLocalDeclaration(ReturnStatementSyntax returnStatement)
        {
            if (returnStatement.Parent?.IsKind(SyntaxKind.Block) == true)
            {
                var block = (BlockSyntax)returnStatement.Parent;
                SyntaxList<StatementSyntax> statements = block.Statements;

                if (statements.Count > 1)
                {
                    int index = statements.IndexOf(returnStatement);

                    if (index > 0)
                    {
                        StatementSyntax statement = statements[index - 1];

                        if (statement.IsKind(SyntaxKind.LocalDeclarationStatement))
                            return (LocalDeclarationStatementSyntax)statement;
                    }
                }
            }

            return null;
        }

        private static VariableDeclaratorSyntax GetVariableDeclarator(LocalDeclarationStatementSyntax localDeclaration)
        {
            VariableDeclarationSyntax declaration = localDeclaration.Declaration;

            if (declaration != null)
            {
                SeparatedSyntaxList<VariableDeclaratorSyntax> variables = declaration.Variables;

                if (variables.Count == 1)
                {
                    VariableDeclaratorSyntax declarator = variables[0];

                    if (declarator.Initializer?.Value != null)
                        return declarator;
                }
            }

            return null;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            LocalDeclarationStatementSyntax localDeclaration,
            CancellationToken cancellationToken)
        {
            var block = (BlockSyntax)localDeclaration.Parent;

            SyntaxList<StatementSyntax> statements = block.Statements;

            int index = statements.IndexOf(localDeclaration);

            var returnStatement = (ReturnStatementSyntax)statements[index + 1];

            ReturnStatementSyntax newReturnStatement = returnStatement
                .WithExpression(localDeclaration.Declaration.Variables[0].Initializer.Value.WithoutTrivia())
                .WithLeadingTrivia(localDeclaration.GetLeadingTrivia())
                .WithTrailingTrivia(returnStatement.GetTrailingTrivia())
                .WithFormatterAnnotation();

            SyntaxList<StatementSyntax> newStatements = statements
                .RemoveAt(index)
                .RemoveAt(index)
                .Insert(index, newReturnStatement);

            BlockSyntax newBlock = block.WithStatements(newStatements);

            return await document.ReplaceNodeAsync(block, newBlock, cancellationToken).ConfigureAwait(false);
        }
    }
}
