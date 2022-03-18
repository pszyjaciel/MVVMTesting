using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Console_MVVMTesting.Messages
{
    public enum TRStatus
    {
        Initialized,
        Connected,
        Disconnected,
        Success,
        Error,
    }

    public class TRSocketStateMessage
    {
        public string MyStateName { get; set; }
        public TRStatus trStatus { get; set; }
        public int TRErrorNumber { get; set; }
        
        //public int SocketHandle { get; set; }
        //public Socket mySocket { get; set; }
        //public decimal ACInVoltage { get; set; }
        public Dictionary<IntPtr, Tuple<string, double, int>> MySocket { get; set; }
        public Dictionary<IntPtr, string> MyInitSocket { get; set; }


        public TRSocketStateMessage(string myStateName, TRStatus trs)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");

            MyStateName = myStateName;
            trStatus = trs;
        }

        public TRSocketStateMessage(string myStateName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(1) " +
              $"({this.GetHashCode():x8})");

            MyStateName = myStateName;
        }

        public TRSocketStateMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(2) " +
              $"({this.GetHashCode():x8})");
        }

        //public TRStatus Response()
        //{
        //    MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] MyStateMessage::Response()  ({this.GetHashCode():x8})");
        //    return lcStatus;
        //}

    }


    internal class TRSocketInitStatusRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
    {
        public TRSocketInitStatusRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] TRSocketInitStatusRequestMessage::TRSocketInitStatusRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class TRSocketCheckPowerSupplyRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
    {
        public TRSocketCheckPowerSupplyRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] TRSocketCheckPowerSupplyRequestMessage::TRSocketCheckPowerSupplyRequestMessage()  ({this.GetHashCode():x8})");
        }
    }


    internal class TRShutdownRequestMessage : RequestMessage<TRSocketStateMessage>
    {
        public TRShutdownRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] TRShutdownRequestMessage::TRShutdownRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class TRSocketTestParameterRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
    {
        public TRSocketTestParameterRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] TRSocketTestParameterRequestMessage::TRSocketTestParameterRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

}
