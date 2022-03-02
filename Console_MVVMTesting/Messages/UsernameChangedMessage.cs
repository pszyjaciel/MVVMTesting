using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    public sealed class UsernameChangedMessage : ValueChangedMessage<string>
    {
        private MyUtils mu = new MyUtils();

        public UsernameChangedMessage(string value) : base(value)
        {
            //mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            //   $"UsernameChangedMessage::UsernameChangedMessage() " +
            //   $"({this.GetHashCode():x8})");
        }
    }
}