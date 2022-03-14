using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    public class LoggedInUserRequestMessageAsync : AsyncRequestMessage<MyUser>
    {
        public LoggedInUserRequestMessageAsync()
        {

            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserRequestMessageAsync::LoggedInUserRequestMessageAsync() ({this.GetHashCode():x8})");
        }

        internal new void Reply(MyUser myUser)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserRequestMessageAsync::Reply(1) ({this.GetHashCode():x8})");
        }

        internal void Reply(string myName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserRequestMessageAsync::Reply(2) ({this.GetHashCode():x8})");
        }
    }

}
