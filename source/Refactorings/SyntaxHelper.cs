﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp
{
    internal static class SyntaxHelper
    {
        public static SyntaxTriviaList GetIndentTrivia(SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            SyntaxTriviaList triviaList = GetNodeForLeadingTrivia(node).GetLeadingTrivia();

            return SyntaxFactory.TriviaList(
                triviaList
                    .Reverse()
                    .TakeWhile(f => f.IsKind(SyntaxKind.WhitespaceTrivia)));
        }

        private static SyntaxNode GetNodeForLeadingTrivia(this SyntaxNode node)
        {
            foreach (SyntaxNode ancestor in node.AncestorsAndSelf())
            {
                if (ancestor.IsKind(SyntaxKind.IfStatement))
                {
                    var parentElse = ancestor.Parent as ElseClauseSyntax;

                    return parentElse ?? ancestor;
                }
                else if (ancestor.IsMemberDeclaration())
                {
                    return ancestor;
                }
                else if (ancestor.IsStatement())
                {
                    return ancestor;
                }
            }

            return node;
        }

        public static string GetSyntaxNodeTitle(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.IfStatement:
                    return "if statement";
                case SyntaxKind.ElseClause:
                    return "else clause";
                case SyntaxKind.ForEachStatement:
                    return "foreach statement";
                case SyntaxKind.ForStatement:
                    return "for statement";
                case SyntaxKind.UsingStatement:
                    return "using statement";
                case SyntaxKind.WhileStatement:
                    return "while statement";
                case SyntaxKind.DoStatement:
                    return "do statement";
                case SyntaxKind.LockStatement:
                    return "lock statement";
                case SyntaxKind.FixedStatement:
                    return "fixed statement";
                case SyntaxKind.CheckedStatement:
                    return "checked statement";
                case SyntaxKind.UncheckedStatement:
                    return "unchecked statement";
                case SyntaxKind.TryStatement:
                    return "try statement";
                case SyntaxKind.UnsafeStatement:
                    return "unsafe statement";
                case SyntaxKind.MethodDeclaration:
                    return "method";
                case SyntaxKind.OperatorDeclaration:
                    return "operator method";
                case SyntaxKind.ConversionOperatorDeclaration:
                    return "conversion method";
                case SyntaxKind.ConstructorDeclaration:
                    return "constructor";
                case SyntaxKind.PropertyDeclaration:
                    return "property";
                case SyntaxKind.IndexerDeclaration:
                    return "indexer";
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.EventFieldDeclaration:
                    return "event";
                case SyntaxKind.FieldDeclaration:
                    return "field";
                case SyntaxKind.NamespaceDeclaration:
                    return "namespace";
                case SyntaxKind.ClassDeclaration:
                    return "class";
                case SyntaxKind.StructDeclaration:
                    return "struct";
                case SyntaxKind.InterfaceDeclaration:
                    return "interface";
                case SyntaxKind.EnumDeclaration:
                    return "enum";
                default:
                    Debug.Assert(false, node.Kind().ToString());
                    return string.Empty;
            }
        }
    }
}
