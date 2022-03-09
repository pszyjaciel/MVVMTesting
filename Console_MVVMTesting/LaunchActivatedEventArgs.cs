using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting
{
    public class LaunchActivatedEventArgs
    {
        

        public LaunchActivatedEventArgs()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"LaunchActivatedEventArgs::LaunchActivatedEventArgs() " +
               $"({this.GetHashCode():x8})");
        }
    }
}