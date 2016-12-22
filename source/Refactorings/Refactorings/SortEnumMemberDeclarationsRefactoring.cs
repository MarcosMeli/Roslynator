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
    internal static class SortEnumMemberDeclarationsRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, EnumDeclarationSyntax enumDeclaration)
        {
            List<EnumMemberDeclarationSyntax> selectedMembers = enumDeclaration.Members
                .SkipWhile(f => context.Span.Start > f.Span.Start)
                .TakeWhile(f => context.Span.End >= f.Span.End)
                .ToList();

            if (selectedMembers.Count > 1)
            {
                if (!EnumMemberDeclarationComparer.IsListSorted(selectedMembers))
                {
                    context.RegisterRefactoring(
                        "Sort enum members by name",
                        cancellationToken => SortByNameAsync(context.Document, enumDeclaration, selectedMembers, cancellationToken));
                }

                if (selectedMembers.All(f => f.EqualsValue?.Value != null))
                {
                    SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                    List<object> values = selectedMembers
                        .Select(f => semanticModel.GetDeclaredSymbol(f, context.CancellationToken))
                        .Where(f => f.HasConstantValue)
                        .Select(f => f.ConstantValue)
                        .ToList();

                    if (selectedMembers.Count == values.Count
                        && !EnumMemberValueComparer.IsListSorted(values))
                    {
                        context.RegisterRefactoring(
                            "Sort enum members by value",
                            cancellationToken => SortByValueAsync(context.Document, enumDeclaration, selectedMembers, values, cancellationToken));
                    }
                }
            }
        }

        private static async Task<Document> SortByNameAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            List<EnumMemberDeclarationSyntax> selectedMembers,
            CancellationToken cancellationToken)
        {
            var comparer = new EnumMemberDeclarationComparer();

            selectedMembers.Sort(comparer);

            SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclaration.Members;

            int firstIndex = members.IndexOf(selectedMembers[0]);
            int lastIndex = members.IndexOf(selectedMembers[selectedMembers.Count - 1]);

            IEnumerable<EnumMemberDeclarationSyntax> newMembers = members
                .Take(firstIndex)
                .Concat(selectedMembers)
                .Concat(members.Skip(lastIndex + 1));

            MemberDeclarationSyntax newNode = enumDeclaration.WithMembers(SyntaxFactory.SeparatedList(newMembers));

            return await document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> SortByValueAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            List<EnumMemberDeclarationSyntax> selectedMembers,
            List<object> values,
            CancellationToken cancellationToken)
        {
            var comparer = new EnumMemberValueComparer();

            var dic = new SortedDictionary<object, EnumMemberDeclarationSyntax>(comparer);

            for (int i = 0; i < values.Count; i++)
                dic.Add(values[i], selectedMembers[i]);

            SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclaration.Members;

            int firstIndex = members.IndexOf(selectedMembers[0]);
            int lastIndex = members.IndexOf(selectedMembers[selectedMembers.Count - 1]);

            IEnumerable<EnumMemberDeclarationSyntax> newMembers = members
                .Take(firstIndex)
                .Concat(dic.Values)
                .Concat(members.Skip(lastIndex + 1));

            MemberDeclarationSyntax newNode = enumDeclaration.WithMembers(SyntaxFactory.SeparatedList(newMembers));

            return await document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}