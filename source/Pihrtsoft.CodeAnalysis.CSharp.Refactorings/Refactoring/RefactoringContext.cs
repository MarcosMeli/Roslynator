﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactoring
{
    internal class RefactoringContext
    {
        private SemanticModel _semanticModel;

        public RefactoringContext(CodeRefactoringContext context, SyntaxNode root)
        {
            BaseContext = context;
            Root = root;
        }

        public SyntaxNode Root { get; }

        public CodeRefactoringContext BaseContext { get; }

        public bool SupportsSemanticModel
        {
            get { return Document.SupportsSemanticModel; }
        }

        public CancellationToken CancellationToken
        {
            get { return BaseContext.CancellationToken; }
        }

        public Document Document
        {
            get { return BaseContext.Document; }
        }

        public TextSpan Span
        {
            get { return BaseContext.Span; }
        }

        public async Task<SemanticModel> GetSemanticModelAsync()
        {
            if (_semanticModel == null)
                _semanticModel = await Document.GetSemanticModelAsync(CancellationToken);

            return _semanticModel;
        }

        public void RegisterRefactoring(
            string title,
            Func<CancellationToken, Task<Document>> createChangedDocument,
            string equivalenceKey = null)
        {
            BaseContext.RegisterRefactoring(CodeAction.Create(title, createChangedDocument, equivalenceKey));
        }

        public void RegisterRefactoring(
            string title,
            Func<CancellationToken, Task<Solution>> createChangedSolution,
            string equivalenceKey = null)
        {
            BaseContext.RegisterRefactoring(CodeAction.Create(title, createChangedSolution, equivalenceKey));
        }

        public void RegisterRefactoring(CodeAction codeAction)
        {
            BaseContext.RegisterRefactoring(codeAction);
        }

        public SyntaxNode FindNode(bool findInsideTrivia = false)
        {
            return Root.FindNode(Span, findInsideTrivia: findInsideTrivia, getInnermostNodeForTie: true);
        }

        public SyntaxToken FindToken()
        {
            return Root.FindToken(Span.Start);
        }

        public SyntaxTrivia FindTrivia()
        {
            return Root.FindTrivia(Span.Start);
        }
    }
}