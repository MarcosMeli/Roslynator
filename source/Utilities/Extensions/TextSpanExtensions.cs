﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator
{
    public static class TextSpanExtensions
    {
        public static bool IsBetweenSpans(this TextSpan span, SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return span.IsBetweenSpans(node.Span, node.FullSpan);
        }

        public static bool IsBetweenSpans<TNode>(this TextSpan span, SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            return span.IsBetweenSpans(list.Span, list.FullSpan);
        }

        public static bool IsBetweenSpans<TNode>(this TextSpan span, SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
            return span.IsBetweenSpans(list.Span, list.FullSpan);
        }

        public static bool IsBetweenSpans(this TextSpan span, SyntaxToken token)
        {
            return span.IsBetweenSpans(token.Span, token.FullSpan);
        }

        private static bool IsBetweenSpans(this TextSpan span, TextSpan innerSpan, TextSpan outerSpan)
        {
            return span.Start >= outerSpan.Start
                && span.Start <= innerSpan.Start
                && span.End >= innerSpan.End
                && span.End <= outerSpan.End;
        }

        public static bool IsContainedInSpanOrBetweenSpans(this TextSpan span, SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            TextSpan innerSpan = node.Span;

            return innerSpan.Contains(span) || span.IsBetweenSpans(innerSpan, node.FullSpan);
        }

        public static bool IsContainedInSpanOrBetweenSpans<TNode>(this TextSpan span, SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            TextSpan innerSpan = list.Span;

            return innerSpan.Contains(span) || span.IsBetweenSpans(innerSpan, list.FullSpan);
        }

        public static bool IsContainedInSpanOrBetweenSpans<TNode>(this TextSpan span, SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
            TextSpan innerSpan = list.Span;

            return innerSpan.Contains(span) || span.IsBetweenSpans(innerSpan, list.FullSpan);
        }

        public static bool IsContainedInSpanOrBetweenSpans(this TextSpan span, SyntaxToken token)
        {
            TextSpan innerSpan = token.Span;

            return innerSpan.Contains(span) || span.IsBetweenSpans(innerSpan, token.FullSpan);
        }

        public static bool IsEmptyAndContainedInSpanOrBetweenSpans(this TextSpan span, SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return IsEmptyAndContainedInSpanOrBetweenSpans(span, node.Span, node.FullSpan);
        }

        public static bool IsEmptyAndContainedInSpanOrBetweenSpans<TNode>(this TextSpan span, SyntaxList<TNode> node) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return IsEmptyAndContainedInSpanOrBetweenSpans(span, node.Span, node.FullSpan);
        }

        public static bool IsEmptyAndContainedInSpanOrBetweenSpans<TNode>(this TextSpan span, SeparatedSyntaxList<TNode> node) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return IsEmptyAndContainedInSpanOrBetweenSpans(span, node.Span, node.FullSpan);
        }

        public static bool IsEmptyAndContainedInSpanOrBetweenSpans(this TextSpan span, SyntaxToken token)
        {
            return IsEmptyAndContainedInSpanOrBetweenSpans(span, token.Span, token.FullSpan);
        }

        private static bool IsEmptyAndContainedInSpanOrBetweenSpans(this TextSpan span, TextSpan innerSpan, TextSpan outerSpan)
        {
            if (span.IsEmpty)
            {
                return innerSpan.Contains(span);
            }
            else
            {
                return span.IsBetweenSpans(innerSpan, outerSpan);
            }
        }
    }
}