using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;


namespace Console_MVVMTesting.Messages
{
    internal class LoggedInUserChangedMessage : ValueChangedMessage<MyUser>
    {
        public LoggedInUserChangedMessage(MyUser myUser) : base(myUser)
        {
            MyUtils.MyConsoleWriteLine($"[{System.DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LoggedInUserChangedMessage::LoggedInUserChangedMessage() ({this.GetHashCode():x8})");
        }
    }
}
