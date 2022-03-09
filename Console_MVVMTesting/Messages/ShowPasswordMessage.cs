using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PInvoke.User32;

namespace Console_MVVMTesting.Messages
{
    public class ShowPasswordMessage : NotificationMessageAction<MessageBoxResult>
    {
        public static EastTesterViewModel sender { get; private set; }
        

        public ShowPasswordMessage(object Sender, Action<MessageBoxResult> callback)
            : base(sender, "GetPassword", callback)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"ShowPasswordMessage::ShowPasswordMessage() " +
                $"({this.GetHashCode():x8})");

        }

    }
}