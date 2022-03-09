using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.Messages
{
    public class GenericMessage<T>
    {

        private MessageOp messageOp;


        public GenericMessage(MessageOp messageOp)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"GenericMessage::GenericMessage(): messageOp: {messageOp} " +
               $"({this.GetHashCode():x8})");

            this.messageOp = messageOp;
        }
    }
}


