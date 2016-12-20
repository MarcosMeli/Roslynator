// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings.EnumWithFlagsAttribute
{
    internal static class GenerateEnumMemberRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, EnumDeclarationSyntax enumDeclaration)
        {
            if (enumDeclaration.BracesSpan().Contains(context.Span))
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                var enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration, context.CancellationToken) as INamedTypeSymbol;

                if (EnumWithFlagsAttributeHelper.IsEnumWithFlagsAttribute(enumSymbol, semanticModel))
                {
                    List<object> values = EnumWithFlagsAttributeHelper.GetExplicitValues(enumDeclaration, semanticModel, context.CancellationToken);

                    object value;
                    if (EnumWithFlagsAttributeHelper.TryGetNewValue(values, enumSymbol, ValueMode.UseAllAvailableValues, out value))
                    {
                        context.RegisterRefactoring(
                            "Generate enum member",
                            cancellationToken => RefactorAsync(context.Document, enumDeclaration, enumSymbol, value, cancellationToken));
                    }

                    object value2;
                    if (EnumWithFlagsAttributeHelper.TryGetNewValue(values, enumSymbol, ValueMode.StartFromHighestExplicitValue, out value2))
                    {
                        context.RegisterRefactoring(
                            "Generate enum member (starting from highest explicit value)",
                            cancellationToken => RefactorAsync(context.Document, enumDeclaration, enumSymbol, value2, cancellationToken));
                    }
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            INamedTypeSymbol enumSymbol,
            object value,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            EqualsValueClauseSyntax equalsValue = EqualsValueClause(ConstantExpression(value));

            string name = NameGenerator.GenerateUniqueEnumMemberName(enumSymbol, "EnumMember");

            SyntaxToken identifier = Identifier(name).WithRenameAnnotation();

            EnumMemberDeclarationSyntax newEnumMember = EnumMemberDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                identifier,
                equalsValue);

            EnumDeclarationSyntax newNode = enumDeclaration.AddMembers(newEnumMember);

            return await document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}