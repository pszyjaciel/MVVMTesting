using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.ViewModels
{
    internal class PasswordDialog
    {
        public PasswordDialog()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] PasswordDialog::PasswordDialog() ({this.GetHashCode():x8})");
        }

        internal string ShowDialog()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] PasswordDialog::ShowDialog() ({this.GetHashCode():x8})");
            return "What is the valid password?";
        }
    }
}