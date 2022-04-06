using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Console_MVVMTesting.Messages
{
    public enum LCStatus
    {
        Initialized,
        Connected,
        Disconnected,
        Success,
        Error,
    }

    public class LCSocketStateMessage
    {
        public string MyStateName { get; set; }
        public LCStatus lcStatus { get; set; }
        public int LCErrorNumber { get; set; }

        //public int SocketHandle { get; set; }
        //public Socket mySocket { get; set; }
        //public decimal ACInVoltage { get; set; }
        public Dictionary<IntPtr, Tuple<string, double, int>> MySocket { get; set; }
        public Dictionary<IntPtr, Tuple<int, string>> SocketInitDict { get; set; }     // socket_handle, error_code, error_description
        public Dictionary<IntPtr, Tuple<UInt16, UInt16>> BatteryStatusDict { get; set; }


        public LCSocketStateMessage(string myStateName, LCStatus lcs)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");

            MyStateName = myStateName;
            lcStatus = lcs;
        }

        public LCSocketStateMessage(string myStateName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");
            MyStateName = myStateName;
        }

        public LCSocketStateMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");
        }

        //public LCStatus Response()
        //{
        //    MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] MyStateMessage::Response()  ({this.GetHashCode():x8})");
        //    return lcStatus;
        //}

    }


    internal class LCSocketInitRequestMessage : AsyncRequestMessage<LCSocketStateMessage>
    {
        public LCSocketInitRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] LCSocketInitRequestMessage::LCSocketInitRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class LCSocketCheckPowerSupplyRequestMessage : AsyncRequestMessage<LCSocketStateMessage>
    {
        public LCSocketCheckPowerSupplyRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] LCSocketCheckPowerSupplyRequestMessage::LCSocketCheckPowerSupplyRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class LCSocketCheckBatteryStatusRequestMessage : AsyncRequestMessage<LCSocketStateMessage>
    {
        public LCSocketCheckBatteryStatusRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"LCSocketCheckBatteryStatusRequestMessage::LCSocketCheckBatteryStatusRequestMessage()  " +
                $"({this.GetHashCode():x8})");
        }
    }


    internal class LCShutdownRequestMessage : AsyncRequestMessage<LCSocketStateMessage>
    {
        public LCShutdownRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] LCShutdownRequestMessage::LCShutdownRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class LCSocketTestParameterRequestMessage : AsyncRequestMessage<LCSocketStateMessage>
    {
        public LCSocketTestParameterRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] LCSocketTestParameterRequestMessage::LCSocketTestParameterRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

}
