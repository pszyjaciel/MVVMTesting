using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.Messages
{
    class InitMessage
    {
        

        public InitMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"InitMessage::InitMessage(1) ({this.GetHashCode():x8})");
        }
    }
}
