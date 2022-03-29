using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Console_MVVMTesting.Helpers;
using System;
using System.Collections.Generic;

namespace Console_MVVMTesting.Messages
{
    public class ProductionStateMessage
    {
        public Dictionary<string, Tuple<int, bool>> ProductionInitDict { get; set; }

        public string MyStateName { get; set; }


        public ProductionStateMessage(string myStateName, ETStatus ets)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");

            MyStateName = myStateName;
        }

        public ProductionStateMessage(string myStateName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");

            MyStateName = myStateName;
        }

        public ProductionStateMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");
        }
    }


    internal class ProductionInitRequestMessage : AsyncRequestMessage<ProductionStateMessage>
    {
        public ProductionInitRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] ProductionInitRequestMessage::ProductionInitRequestMessage()  ({this.GetHashCode():x8})");
        }
    }


    internal class NumberOfSetsValueChangedMessage : ValueChangedMessage<int>
    {
        public NumberOfSetsValueChangedMessage(int value) : base(value)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] NumberOfSetsValueChangedMessage::NumberOfSetsValueChangedMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class BatteryTypeValueChangedMessage : ValueChangedMessage<string>
    {
        public BatteryTypeValueChangedMessage(string value) : base(value)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] BatteryTypeValueChangedMessage::BatteryTypeValueChangedMessage()  ({this.GetHashCode():x8})");
        }
    }

}