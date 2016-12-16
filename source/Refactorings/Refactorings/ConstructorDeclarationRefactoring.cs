﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ConstructorDeclarationRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, ConstructorDeclarationSyntax constructorDeclaration)
        {
            if (context.IsAnyRefactoringEnabled(RefactoringIdentifiers.MarkMemberAsStatic, RefactoringIdentifiers.MarkAllMembersAsStatic)
                && constructorDeclaration.Span.Contains(context.Span)
                && MarkMemberAsStaticRefactoring.CanRefactor(constructorDeclaration))
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.MarkMemberAsStatic))
                {
                    context.RegisterRefactoring(
                        "Mark constructor as static",
                        cancellationToken => MarkMemberAsStaticRefactoring.RefactorAsync(context.Document, constructorDeclaration, cancellationToken));
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.MarkAllMembersAsStatic))
                    MarkAllMembersAsStaticRefactoring.RegisterRefactoring(context, (ClassDeclarationSyntax)constructorDeclaration.Parent);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.CopyDocumentationCommentFromBaseMember)
                && constructorDeclaration.HeaderSpanIncludingInitializer().Contains(context.Span))
            {
                await CopyDocumentationCommentFromBaseMemberRefactoring.ComputeRefactoringAsync(context, constructorDeclaration).ConfigureAwait(false);
            }
        }
    }
}