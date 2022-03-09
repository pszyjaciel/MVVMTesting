using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Models;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    public class PropertyChangedPostMessage : PropertyChangedMessage<Post>
    {
        private const string consoleColor = "LRED";

        public PropertyChangedPostMessage(object sender, string propertyName, Post oldValue, Post newValue) : base(sender, propertyName, oldValue, newValue)
        {
            MyUtils.MyConsoleWriteLine(consoleColor, $"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"PropertyChangedPostMessage::PropertyChangedPostMessage()  ({this.GetHashCode():x8})");
        }
    }
}
