using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    internal class MyObjectChangedMessage : ValueChangedMessage<MyObject>
    {
        private MyUtils mu = new MyUtils();


        internal MyObjectChangedMessage(MyObject value) : base(value)
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            $"MyObjectChangedMessage::MyObjectChangedMessage() " +
            $"({this.GetHashCode():x8})");

        }
    }
}
