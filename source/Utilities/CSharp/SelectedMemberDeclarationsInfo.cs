// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp
{
    public class SelectedMemberDeclarationsInfo : SelectedNodesInfo<MemberDeclarationSyntax>
    {
        private SelectedMemberDeclarationsInfo(MemberDeclarationSyntax parentMember, SyntaxList<MemberDeclarationSyntax> members, TextSpan span)
             : base(members, span)
        {
            ParentMember = parentMember;
        }

        public MemberDeclarationSyntax ParentMember { get; }

        public static SelectedMemberDeclarationsInfo Create(NamespaceDeclarationSyntax namespaceDeclaration, TextSpan span)
        {
            if (namespaceDeclaration == null)
                throw new ArgumentNullException(nameof(namespaceDeclaration));

            return new SelectedMemberDeclarationsInfo(namespaceDeclaration, namespaceDeclaration.Members, span);
        }

        public static SelectedMemberDeclarationsInfo Create(ClassDeclarationSyntax classDeclaration, TextSpan span)
        {
            if (classDeclaration == null)
                throw new ArgumentNullException(nameof(classDeclaration));

            return new SelectedMemberDeclarationsInfo(classDeclaration, classDeclaration.Members, span);
        }

        public static SelectedMemberDeclarationsInfo Create(StructDeclarationSyntax structDeclaration, TextSpan span)
        {
            if (structDeclaration == null)
                throw new ArgumentNullException(nameof(structDeclaration));

            return new SelectedMemberDeclarationsInfo(structDeclaration, structDeclaration.Members, span);
        }

        public static SelectedMemberDeclarationsInfo Create(InterfaceDeclarationSyntax interfaceDeclaration, TextSpan span)
        {
            if (interfaceDeclaration == null)
                throw new ArgumentNullException(nameof(interfaceDeclaration));

            return new SelectedMemberDeclarationsInfo(interfaceDeclaration, interfaceDeclaration.Members, span);
        }
    }
}
