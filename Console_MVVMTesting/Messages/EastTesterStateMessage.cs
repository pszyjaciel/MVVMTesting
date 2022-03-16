using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    public enum ETStatus
    {
        Initialized,
        Connected,
        Disconnected,
        Success,
        Error,
    }


    public class EastTesterStateMessage
    {
        public string MyStateName { get; set; }
        public ETStatus etStatus { get; set; }
        public int ETErrorNumber { get; set; }


        public EastTesterStateMessage(string myStateName, ETStatus ets)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");

            MyStateName = myStateName;
            etStatus = ets;
        }

        public EastTesterStateMessage(string myStateName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(1) " +
              $"({this.GetHashCode():x8})");

            MyStateName = myStateName;
        }

        public EastTesterStateMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"MyUser::MyUser(2) " +
              $"({this.GetHashCode():x8})");
        }

        public ETStatus Response()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] MyStateMessage::Response()  ({this.GetHashCode():x8})");
            return etStatus;
        }
    }


    internal class EastTesterStatusRequestMessage : AsyncRequestMessage<EastTesterStateMessage>
    {
        public EastTesterStatusRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] EastTesterStatusRequestMessage::EastTesterStatusRequestMessage()  ({this.GetHashCode():x8})");
        }
    }


    internal class EastTesterShutdownRequestMessage : RequestMessage<EastTesterStateMessage>
    {
        public EastTesterShutdownRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] EastTesterShutdownRequestMessage::EastTesterShutdownRequestMessage()  ({this.GetHashCode():x8})");
        }

    }





}