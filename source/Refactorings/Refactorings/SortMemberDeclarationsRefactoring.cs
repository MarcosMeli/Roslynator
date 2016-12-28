// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
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
                ImmutableArray<MemberDeclarationSyntax> selectedMembers = info.SelectedNodes().ToImmutableArray();

                SyntaxKind kind = GetSingleKindOrDefault(selectedMembers);

                if (kind != SyntaxKind.None)
                {
                    if (MemberDeclarationComparer.CanBeSortedAlphabetically(kind))
                    {
                        ComputeRefactoring(
                            context,
                            MemberDeclarationSortMode.ByKindThenByName,
                            "Sort members by name",
                            info,
                            selectedMembers);
                    }
                }
                else
                {
                    ComputeRefactoring(
                        context,
                        MemberDeclarationSortMode.ByKind,
                        "Sort members by kind",
                        info,
                        selectedMembers);

                    ComputeRefactoring(
                        context,
                        MemberDeclarationSortMode.ByKindThenByName,
                        "Sort members by kind then by name",
                        info,
                        selectedMembers);
                }
            }
        }

        private static void ComputeRefactoring(RefactoringContext context, MemberDeclarationSortMode sortMode, string title, SelectedMemberDeclarationsInfo info, ImmutableArray<MemberDeclarationSyntax> selectedMembers)
        {
            if (!MemberDeclarationComparer.IsListSorted(selectedMembers, sortMode))
            {
                context.RegisterRefactoring(
                    title,
                    cancellationToken => RefactorAsync(context.Document, info, selectedMembers, sortMode, cancellationToken));
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            SelectedMemberDeclarationsInfo info,
            ImmutableArray<MemberDeclarationSyntax> selectedMembers,
            MemberDeclarationSortMode sortMode,
            CancellationToken cancellationToken)
        {
            var comparer = new MemberDeclarationComparer(sortMode);

            IEnumerable<MemberDeclarationSyntax> newMembers = info.Nodes
                .Take(info.FirstSelectedNodeIndex)
                .Concat(selectedMembers.OrderBy(f => f, comparer))
                .Concat(info.Nodes.Skip(info.LastSelectedNodeIndex + 1));

            MemberDeclarationSyntax newNode = info.ParentMember.SetMembers(SyntaxFactory.List(newMembers));

            return await document.ReplaceNodeAsync(info.ParentMember, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static SyntaxKind GetSingleKindOrDefault(ImmutableArray<MemberDeclarationSyntax> members)
        {
            SyntaxKind kind = members.First().Kind();

            for (int i = 1; i < members.Length; i++)
            {
                if (members[i].Kind() != kind)
                    return SyntaxKind.None;
            }

            return kind;
        }
    }
}