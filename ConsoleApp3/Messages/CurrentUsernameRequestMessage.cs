using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    // A sample request message to get the current username
    public sealed class CurrentUsernameRequestMessage : RequestMessage<string>
    {
        private MyUtils mu;
        private const string consoleColor = "PINK";

        public CurrentUsernameRequestMessage()
        {
            mu = new MyUtils();

            mu.MyConsoleWriteLine(consoleColor, $"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"CurrentUsernameRequestMessage::CurrentUsernameRequestMessage() " +
              $"({this.GetHashCode():x8})");
        }
    }
}