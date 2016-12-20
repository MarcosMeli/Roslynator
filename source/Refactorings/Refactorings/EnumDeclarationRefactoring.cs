// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class EnumDeclarationRefactoring
    {
        public static void ComputeRefactoring(RefactoringContext context, EnumDeclarationSyntax enumDeclaration)
        {
            ExtractTypeDeclarationToNewFileRefactoring.ComputeRefactorings(context, enumDeclaration);
        }
    }
}