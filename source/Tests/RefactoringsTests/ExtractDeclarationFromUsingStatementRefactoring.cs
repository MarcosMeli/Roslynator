﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Collections;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ExtractDeclarationFromUsingStatementRefactoring
    {
        public void SomeMethod()
        {
            Stream stream = null;

            using (var streamReader = new StreamReader(stream))
            {
            }

            using (var streamReader = new BitArray().GetEnumerator())
            {
            }
        }
    }
}
