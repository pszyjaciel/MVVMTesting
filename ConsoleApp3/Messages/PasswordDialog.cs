using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.ViewModels
{
    internal class PasswordDialog
    {
        private MyUtils mu;
        public PasswordDialog()
        {
            mu = new MyUtils();
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] PasswordDialog::PasswordDialog() ({this.GetHashCode():x8})");
        }

        internal string ShowDialog()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] PasswordDialog::ShowDialog() ({this.GetHashCode():x8})");
            return "What is the valid password?";
        }
    }
}