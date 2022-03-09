using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Messages
{
    //public class LoggedInUserRequestMessage : RequestMessage<MyUser>
    //{
    //}

    public class LoggedInUserRequestMessage : AsyncRequestMessage<MyUser>
    {
        public LoggedInUserRequestMessage()
        {

            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserRequestMessage::LoggedInUserRequestMessage() ({this.GetHashCode():x8})");
        }

        internal new void Reply(MyUser myUser)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserRequestMessage::Reply(1) ({this.GetHashCode():x8})");
        }

        internal void Reply(string myName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserRequestMessage::Reply(2) ({this.GetHashCode():x8})");
        }
    }

}
