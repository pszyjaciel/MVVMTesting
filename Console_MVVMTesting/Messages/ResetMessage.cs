using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    public class ResetMessage
    {
        

        public ResetMessage()
        {
            //MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            //   $"ResetMessage::ResetMessage(1) ({this.GetHashCode():x8})");
        }


        public ResetMessage(bool MajWjenkszyBul)
        {
            //MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            //   $"ResetMessage::ResetMessage(2) ({this.GetHashCode():x8})");
        }



        //public ResetMessage() : base(MessageOp.Reset) { }
        //public ResetMessage(MessageOp operation) : base(operation)      // base odnosi sie do OperationMessage
        //{
        //    MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
        //       $"ResetMessage::ResetMessage(2) ({this.GetHashCode():x8})");
        //}

    }
}
