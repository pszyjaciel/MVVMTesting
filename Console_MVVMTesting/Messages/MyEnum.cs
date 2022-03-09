using Console_MVVMTesting.Helpers;
using System;


namespace Console_MVVMTesting.Messages
{
    class MyEnum
    {
        
        private bool _myBool;
        

        public bool majBul { get; set; }
        public static bool majRzal { get; set; }

        public MyEnum()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                   $"MyEnum::MyEnum(1)  ({this.GetHashCode():x8})");
        }

        public MyEnum(bool myBool)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                   $"MyEnum::MyEnum(2) myBool: {myBool}  ({this.GetHashCode():x8})");

            this._myBool = myBool;
            this.majBul = _myBool;
        }

    }
}
