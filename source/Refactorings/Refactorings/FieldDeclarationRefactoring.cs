﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class FieldDeclarationRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, FieldDeclarationSyntax fieldDeclaration)
        {
            if (fieldDeclaration.Modifiers.Contains(SyntaxKind.ConstKeyword))
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceConstantWithField)
                    && fieldDeclaration.Span.Contains(context.Span))
                {
                    context.RegisterRefactoring(
                        "Replace constant with field",
                        cancellationToken => ReplaceConstantWithFieldRefactoring.RefactorAsync(context.Document, fieldDeclaration, cancellationToken));
                }
            }
            else if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceFieldWithConstant)
                && fieldDeclaration.Modifiers.Contains(SyntaxKind.ReadOnlyKeyword)
                && fieldDeclaration.Modifiers.Contains(SyntaxKind.StaticKeyword)
                && fieldDeclaration.Span.Contains(context.Span))
            {
                if (await ReplaceFieldWithConstantRefactoring.CanRefactorAsync(context, fieldDeclaration).ConfigureAwait(false))
                {
                    context.RegisterRefactoring(
                        "Replace field with constant",
                        cancellationToken => ReplaceFieldWithConstantRefactoring.RefactorAsync(context.Document, fieldDeclaration, cancellationToken));
                }
            }

            if (context.IsAnyRefactoringEnabled(RefactoringIdentifiers.MarkMemberAsStatic, RefactoringIdentifiers.MarkAllMembersAsStatic)
                && fieldDeclaration.Span.Contains(context.Span)
                && MarkMemberAsStaticRefactoring.CanRefactor(fieldDeclaration))
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.MarkMemberAsStatic))
                {
                    context.RegisterRefactoring(
                   "Mark field as static",
                   cancellationToken => MarkMemberAsStaticRefactoring.RefactorAsync(context.Document, fieldDeclaration, cancellationToken));
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.MarkAllMembersAsStatic))
                    MarkAllMembersAsStaticRefactoring.RegisterRefactoring(context, (ClassDeclarationSyntax)fieldDeclaration.Parent);
            }
        }
    }
}
