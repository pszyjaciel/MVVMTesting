using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using PInvoke;
using System;

namespace Console_MVVMTesting.Messages
{
    public class NotificationMessageAction<T>
    {
        private EastTesterViewModel eastTesterViewModel;
        private string v;
        private Action<object> p;
        private EastTesterViewModel sender;
        private Action<User32.MessageBoxResult> callback;
        

        public NotificationMessageAction(EastTesterViewModel eastTesterViewModel, string v, Action<object> p)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"NotificationMessageAction::NotificationMessageAction(1) ({this.GetHashCode():x8})");

            this.eastTesterViewModel = eastTesterViewModel;
            this.v = v;
            this.p = p;
        }

        public NotificationMessageAction(EastTesterViewModel sender, string v, Action<User32.MessageBoxResult> callback)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"NotificationMessageAction::NotificationMessageAction(2) " +
                $"({this.GetHashCode():x8})");

            this.sender = sender;
            this.v = v;
            this.callback = callback;
        }

        public string Notification { get; internal set; }

        internal void Execute(object result)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"NotificationMessageAction::Execute() ({this.GetHashCode():x8})");

            if (result == null) return;
            if (result is EastTesterViewModel)
            {
                MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                    $"NotificationMessageAction::Execute(): result is EastTesterViewModel " +
                    $"({this.GetHashCode():x8})");
            }
            else if (result is EastTesterViewModel)
                this.sender = (EastTesterViewModel)result;
        }
    }
}