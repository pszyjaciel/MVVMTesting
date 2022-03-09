using Console_MVVMTesting.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Messages
{
    public class OperationMessage<messageOp>
    {
        
//        public MessageOp messageOp { get; set; }

        public OperationMessage()
        {
            //MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            //    $"OperationMessage::OperationMessage(1) " +
            //    $"({this.GetHashCode():x8})");
        }

        public OperationMessage(bool majBul)
        {
            //MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            //    $"OperationMessage::OperationMessage(2) " +
            //    $"({this.GetHashCode():x8})");
        }

        //public OperationMessage(object operation)
        //{
        //    Operation = operation;
        //}

        //public object Operation { get; }
    }
}
