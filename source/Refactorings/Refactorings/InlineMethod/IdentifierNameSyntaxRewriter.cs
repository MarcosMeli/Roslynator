﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings.InlineMethod
{
    internal class IdentifierNameSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<IdentifierNameSyntax, ExpressionSyntax> _dic;

        public IdentifierNameSyntaxRewriter(Dictionary<IdentifierNameSyntax, ExpressionSyntax> dic)
        {
            _dic = dic;
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            ExpressionSyntax expression;

            if (_dic.TryGetValue(node, out expression))
            {
                _dic.Remove(node);

                if (SyntaxAnalyzer.AreParenthesesRedundantOrInvalid(node, expression.Kind()))
                {
                    return expression;
                }
                else
                {
                    return expression.Parenthesize(cutCopyTrivia: true);
                }
            }
            else
            {
                return base.VisitIdentifierName(node);
            }
        }
    }
}
