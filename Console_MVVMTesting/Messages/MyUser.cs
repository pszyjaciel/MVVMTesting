using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.Messages
{
    public class MyUser
    {
        public string _myName { get; private set; }
        

        public MyUser(string myName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(1) " +
              $"({this.GetHashCode():x8})");

            _myName = myName;
        }

        public MyUser()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(2) " +
              $"({this.GetHashCode():x8})");
        }
    }
}