using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.Messages
{
    public class MyUser
    {
        public string _myName { get; private set; }
        private MyUtils mu = new MyUtils();

        public MyUser(string myName)
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(1) " +
              $"({this.GetHashCode():x8})");

            _myName = myName;
        }

        public MyUser()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(2) " +
              $"({this.GetHashCode():x8})");
        }
    }
}