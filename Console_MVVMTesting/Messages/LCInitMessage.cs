using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Messages
{
    class LCInitMessage
    {
        public LCInitMessage()
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] InitLCMessage::InitLCMessage()  ({this.GetHashCode():x8})");
        }
    }
}
