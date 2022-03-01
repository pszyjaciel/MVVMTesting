using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting
{
    public class LaunchActivatedEventArgs
    {
        private MyUtils mu = new MyUtils();

        public LaunchActivatedEventArgs()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"LaunchActivatedEventArgs::LaunchActivatedEventArgs() " +
               $"({this.GetHashCode():x8})");
        }
    }
}