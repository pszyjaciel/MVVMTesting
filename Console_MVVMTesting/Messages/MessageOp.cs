using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.Messages
{
    //public enum MessageOp
    //{
    //    Reset,
    //    Exit,
    //    Close
    //}

    public class MessageOp
    {
        public static bool Reset { get; set; }
        public static bool Exit { get; set; }

        public string _reset { get; set; }
        private string _exit { get; set; }

        private MyUtils mu = new MyUtils();


        public MessageOp()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"MessageOp::MessageOp()  ({this.GetHashCode():x8})");
        }

        public MessageOp(string Reset)
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"MessageOp::MessageOp(): Reset: {Reset} ({this.GetHashCode():x8})");
            this._reset = Reset;
        }


        public MessageOp(string Reset, string Exit)
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"MessageOp::MessageOp(): Reset: {Reset}, Exit: {Exit} ({this.GetHashCode():x8})");
            this._reset = Reset;
            this._exit = Exit;
        }
    }
}