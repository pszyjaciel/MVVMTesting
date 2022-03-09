using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    public sealed class UsernameChangedMessage : ValueChangedMessage<string>
    {
        

        public UsernameChangedMessage(string value) : base(value)
        {
            //MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            //   $"UsernameChangedMessage::UsernameChangedMessage() " +
            //   $"({this.GetHashCode():x8})");
        }
    }
}