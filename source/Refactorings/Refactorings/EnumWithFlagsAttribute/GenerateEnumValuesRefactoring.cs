// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.Refactorings.EnumWithFlagsAttribute
{
    internal static class GenerateEnumValuesRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, EnumDeclarationSyntax enumDeclaration)
        {
            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            var enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration, context.CancellationToken) as INamedTypeSymbol;

            if (EnumWithFlagsAttributeHelper.IsEnumWithFlagsAttribute(enumSymbol, semanticModel))
            {
                SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclaration.Members;

                if (members.Any(f => f.EqualsValue == null))
                {
                    context.RegisterRefactoring(
                        "Generate enum values",
                        cancellationToken => RefactorAsync(context.Document, enumDeclaration, enumSymbol, ValueMode.UseAllAvailableValues, cancellationToken: cancellationToken));

                    if (members.Any(f => f.EqualsValue != null))
                    {
                        context.RegisterRefactoring(
                       "Generate enum values (starting from highest explicit value)",
                       cancellationToken => RefactorAsync(context.Document, enumDeclaration, enumSymbol, ValueMode.StartFromHighestExplicitValue, cancellationToken));
                    }
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            INamedTypeSymbol enumSymbol,
            ValueMode mode,
            CancellationToken cancellationToken)
        {
            SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclaration.Members;

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            List<object> values = EnumWithFlagsAttributeHelper.GetExplicitValues(enumDeclaration, semanticModel, cancellationToken);

            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].EqualsValue == null)
                {
                    object value;
                    if (EnumWithFlagsAttributeHelper.TryGetNewValue(values, enumSymbol, mode, out value))
                    {
                        values.Add(value);

                        EqualsValueClauseSyntax equalsValue = EqualsValueClause(CSharpFactory.ConstantExpression(value));

                        EnumMemberDeclarationSyntax newMember = members[i]
                            .WithEqualsValue(equalsValue)
                            .WithFormatterAnnotation();

                        members = members.ReplaceAt(i, newMember);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            EnumDeclarationSyntax newNode = enumDeclaration.WithMembers(members);

            return await document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}