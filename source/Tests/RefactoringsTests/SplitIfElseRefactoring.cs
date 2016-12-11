// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class SplitIfElseRefactoring
    {
        public static string Foo()
        {
            object x = null;

            if (x is int)
            {
                return "Int32";
            }
            else if (x is long)
            {
                return "Int64";
            }

            if (x is int)
                return "Int32";
            else if (x is long)
                return "Int64";

            if (x is int)
            {
                return "Int32";
            }
            else if (x is long)
            {
                return "Int64";
            }
            else
            {
                return "";
            }
        }
    }
}
