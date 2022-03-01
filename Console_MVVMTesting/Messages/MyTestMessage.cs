using Console_MVVMTesting.Helpers;
using System;


namespace Console_MVVMTesting.Messages
{
    internal class MyTestMessage<MyEnum>
    {
        private MyUtils mu = new MyUtils();

        public MyTestMessage()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                   $"MyTestMessage::MyTestMessage()  ({this.GetHashCode():x8})");

        }
    }
}



