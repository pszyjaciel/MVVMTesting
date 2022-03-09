using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    internal class MyObjectChangedMessage : ValueChangedMessage<MyObject>
    {
        


        internal MyObjectChangedMessage(MyObject value) : base(value)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            $"MyObjectChangedMessage::MyObjectChangedMessage() " +
            $"({this.GetHashCode():x8})");

        }
    }
}
