using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Messages
{
    class LCCloseMessage
    {
        public LCCloseMessage()
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] CloseLCMessage::CloseLCMessage()  ({this.GetHashCode():x8})");
        }
    }
}
