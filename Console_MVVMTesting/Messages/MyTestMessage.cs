using Console_MVVMTesting.Helpers;
using System;


namespace Console_MVVMTesting.Messages
{
    internal class MyTestMessage<MyEnum>
    {
        

        public MyTestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                   $"MyTestMessage::MyTestMessage()  ({this.GetHashCode():x8})");

        }
    }
}



