using Console_MVVMTesting.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Messages
{
    public class CasualtyMessage
    {
        private MyUtils mu = new MyUtils();
        public CasualtyMessage()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"CasualtyMessage::CasualtyMessage() " +
               $"({this.GetHashCode():x8})");
        }
    }
}



