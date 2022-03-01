using EnvDTE;
using System;

namespace Console_MVVMTesting.Helpers
{
    internal class MyUtilsBase
    {
        internal static Solution Call(Func<object> p)
        {
            Console.WriteLine("MyUtils::call()");
            return null;
        }
    }
}