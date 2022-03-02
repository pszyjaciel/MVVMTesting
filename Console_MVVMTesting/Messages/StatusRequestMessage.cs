using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Messages
{
    public class StatusRequestMessage : RequestMessage<bool>
    {
        private MyUtils mu;
        private const string consoleColor = "DWHITE";

        public StatusRequestMessage()
        {
            mu = new MyUtils();

            //mu.MyConsoleWriteLine(consoleColor, $"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
            //  $"StatusRequestMessage::StatusRequestMessage()  ({this.GetHashCode():x8})");
        }
    }
}
