// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class SortMemberDeclarationsRefactoring
    {
        private const string Title = "Sort members";

        public static void ComputeRefactoring(RefactoringContext context, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            SelectedMemberDeclarationsInfo info = SelectedMemberDeclarationsInfo.Create(namespaceDeclaration, context.Span);

            ComputeRefactoring(context, info);
        }

        public static void ComputeRefactoring(RefactoringContext context, ClassDeclarationSyntax classDeclaration)
        {
            SelectedMemberDeclarationsInfo info = SelectedMemberDeclarationsInfo.Create(classDeclaration, context.Span);

            ComputeRefactoring(context, info);
        }

        public static void ComputeRefactoring(RefactoringContext context, StructDeclarationSyntax structDeclaration)
        {
            SelectedMemberDeclarationsInfo info = SelectedMemberDeclarationsInfo.Create(structDeclaration, context.Span);

            ComputeRefactoring(context, info);
        }

        public static void ComputeRefactoring(RefactoringContext context, InterfaceDeclarationSyntax interfaceDeclaration)
        {
            SelectedMemberDeclarationsInfo info = SelectedMemberDeclarationsInfo.Create(interfaceDeclaration, context.Span);

            ComputeRefactoring(context, info);
        }

        private static void ComputeRefactoring(RefactoringContext context, SelectedMemberDeclarationsInfo info)
        {
            if (info.AreManySelected)
            {
                List<MemberDeclarationSyntax> selectedMembers = info.SelectedNodes().ToList();

                SyntaxKind kind = GetSingleOrNoneKind(selectedMembers);

                if (kind != SyntaxKind.None)
                {
                    var sortMode = MemberDeclarationSortMode.ByKindThenAlphabetically;

                    if (MemberDeclarationComparer.CanBeSortedAlphabetically(kind)
                        && !MemberDeclarationComparer.IsListSorted(selectedMembers, sortMode))
                    {
                        context.RegisterRefactoring(
                            Title,
                            cancellationToken => RefactorAsync(context.Document, info, selectedMembers, sortMode, cancellationToken));
                    }
                }
                else
                {
                    var sortMode = MemberDeclarationSortMode.ByKind;

                    if (!MemberDeclarationComparer.IsListSorted(selectedMembers, sortMode))
                    {
                        context.RegisterRefactoring(
                            Title,
                            cancellationToken => RefactorAsync(context.Document, info, selectedMembers, sortMode, cancellationToken));
                    }
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            SelectedMemberDeclarationsInfo info,
            List<MemberDeclarationSyntax> selectedMembers,
            MemberDeclarationSortMode sortMode,
            CancellationToken cancellationToken)
        {
            var comparer = new MemberDeclarationComparer(sortMode);

            selectedMembers.Sort(comparer);

            IEnumerable<MemberDeclarationSyntax> newMembers = info.Nodes
                .Take(info.FirstSelectedNodeIndex)
                .Concat(selectedMembers)
                .Concat(info.Nodes.Skip(info.LastSelectedNodeIndex + 1));

            MemberDeclarationSyntax newNode = info.ParentMember.SetMembers(SyntaxFactory.List(newMembers));

            return await document.ReplaceNodeAsync(info.ParentMember, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static SyntaxKind GetSingleOrNoneKind(List<MemberDeclarationSyntax> members)
        {
            var kind = SyntaxKind.None;

            using (List<MemberDeclarationSyntax>.Enumerator en = members.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    kind = en.Current.Kind();

                    while (en.MoveNext())
                    {
                        if (en.Current.Kind() != kind)
                            return SyntaxKind.None;
                    }
                }
            }

            return kind;
        }
    }
}