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

    public enum BatteryModeEnum : UInt16
    {

    }

    public enum BatteryStatusEnum : UInt16
    {

    }

    public enum SafetyAlertEnum : UInt32
    {
        Overcharge = 0x00100000,
        ChargeTimeoutSuspend = 0x00080000,
        PrechargeTimeoutSuspend = 0x00020000,
        OvercurrentDuringDischargeLatch = 0x00004000,
        OvertemperatureFault = 0x00002000,
        AFEAlert = 0x00001000,
        UndertemperatureDuringDischarge = 0x00000800,
        UndertemperatureDuringCharge = 0x00000400,
        OvertemperatureDuringDischarge = 0x00000200,
        OvertemperatureDuringCharge = 0x00000100,
        ShortCircuitDuringDischargeLatch = 0x00000080,
        ShortCircuitDuringDischarge = 0x00000040,
        OverloadDuringDischargeLatch = 0x00000020,
        OverloadDuringDischarge = 0x00000010,
        OvercurrentDuringDischarge = 0x00000008,
        OvercurrentDuringCharge = 0x00000004,
        CellOvervoltage = 0x00000002,
        CellUndervoltage = 0x00000001,
        NoError = 0x00000000,
    }

    public enum SafetyStatusEnum : UInt32
    {
        NoError,
        Overcharge,
        ChargeTimeout,
        PrechargeTimeout,
        OvercurrentDuringDischargeLatch,
        OvertemperatureFault,
        AFEAlert,
        UndertemperatureDuringDischarge,
        UndertemperatureDuringCharge,
        OvertemperatureDuringDischarge,
        OvertemperatureDuringCharge,
        ShortCircuitDuringDischargeLatch,
        ShortCircuitDuringDischarge,
        OverloadDuringDischargeLatch,
        OverloadDuringDischarge,
        OvercurrentDuringDischarge,
        OvercurrentDuringCharge,
        CellOvervoltage,
        CellUndervoltage,
    }

    public enum PFStatusEnum : UInt32
    {
        NoError,
        DataFlashWearoutFailure,
        InstructionFlashChecksumFailure,
        SafetyOvertemperatureFETFailure,
        OpenThermistorTS3Failure,
        OpenThermistorTS2Failure,
        OpenThermistorTS1Failure,
        CompanionBQ769x0AFEXREADYFailure,
        CompanionBQ769x0AFEOverrideFailure,
        AFECommunicationFailure,
        AFERegisterFailure,
        DischargeFETFailure,
        ChargeFETFailure,
        VoltageImbalancewhilepackisatrestfailure,
        SafetyOvertemperatureCellFailure,
        SafetyOvercurrentinDischarge,
        SafetyOvercurrentinCharge,
        SafetyCellOvervoltageFailure,
        SafetyCellUndervoltageFailure
    }


    public enum PFAlertEnum : UInt16
    {
        NoError,
        SafetyOvertemperatureFETFailure,
        OpenThermistorTS3Failure,
        OpenThermistorTS2Failure,
        OpenThermistorTS1Failure,
        CompanionBQ769x0AFEXREADYFailure,
        CompanionBQ769x0AFEOverrideFailure,
        AFECommunicationFailure,
        AFERegisterFailure,
        DischargeFETFailure,
        ChargeFETFailure,
        VoltageImbalanceWhilePackIsAtRestFailure,
        SafetyOvertemperatureCellFailure,
        SafetyOvercurrentInDischarge,
        SafetyOvercurrentInCharge,
        SafetyCellOvervoltageFailure,
        SafetyCellUndervoltageFailure,
    }


    public class TRSocketStateMessage
    {
        public string MyStateName { get; set; }
        public TRStatus trStatus { get; set; }
        public int TRErrorNumber { get; set; }

        //public int SocketHandle { get; set; }
        //public Socket mySocket { get; set; }
        //public decimal ACInVoltage { get; set; }
        public Dictionary<IntPtr, Tuple<string, double, int>> CheckPowerSupplyDict { get; set; }
        public Dictionary<IntPtr, Tuple<int, string>> SocketInitDict { get; set; }     // socket_handle, error_code, error_description
        //public Dictionary<IntPtr, Tuple<SafetyAlertEnum, SafetyStatusEnum, PFAlertEnum, PFStatusEnum>> BatteryStatusAndAlarmsDict { get; set; }
        //public Dictionary<IntPtr, Tuple<BatteryMode, BatteryStatus, SafetyAlertEnum, SafetyStatusEnum, PFAlertEnum, PFStatusEnum>> BatteryStatusAndAlarmsDict { get; set; }
        public Dictionary<IntPtr, Tuple<UInt16, UInt16>> BatteryStatusDict { get; set; }
        public Dictionary<IntPtr, Tuple<UInt32, UInt32, UInt16, UInt32>> BatteryAlarmsDict { get; set; }


        public TRSocketStateMessage(string myStateName, TRStatus trs)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");

            MyStateName = myStateName;
            trStatus = trs;
        }

        public TRSocketStateMessage(string myStateName)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");
            MyStateName = myStateName;
        }

        public TRSocketStateMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]  ({this.GetHashCode():x8})");
        }
    }

    internal class CheckBatteryStatusRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
    {
        public CheckBatteryStatusRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] CheckBatteryStatusRequestMessage::CheckBatteryStatusRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class CheckBatteryAlarmsRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
    {
        public CheckBatteryAlarmsRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] CheckBatteryAlarmsRequestMessage::CheckBatteryAlarmsRequestMessage()  ({this.GetHashCode():x8})");
        }
    }


    internal class TRSocketInitRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
    {
        public TRSocketInitRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] TRSocketInitRequestMessage::TRSocketInitRequestMessage()  ({this.GetHashCode():x8})");
        }
    }

    internal class TRSocketCheckPowerSupplyRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
    {
        public TRSocketCheckPowerSupplyRequestMessage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] TRSocketCheckPowerSupplyRequestMessage::TRSocketCheckPowerSupplyRequestMessage()  ({this.GetHashCode():x8})");
        }
    }


    internal class TRShutdownRequestMessage : AsyncRequestMessage<TRSocketStateMessage>
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
