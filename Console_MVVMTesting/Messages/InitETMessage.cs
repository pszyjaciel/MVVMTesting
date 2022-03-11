using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.Messages
{
    class InitETMessage
    {
        public InitETMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"InitETMessage::InitETMessage() ({this.GetHashCode():x8})");
        }
    }
}
