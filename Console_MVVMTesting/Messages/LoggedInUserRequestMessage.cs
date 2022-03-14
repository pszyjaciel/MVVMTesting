using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Messages
{
    public class LoggedInUserRequestMessage : RequestMessage<MyUser>
    {
        public LoggedInUserRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserRequestMessage::LoggedInUserRequestMessage() ({this.GetHashCode():x8})");
        }
    }

}
