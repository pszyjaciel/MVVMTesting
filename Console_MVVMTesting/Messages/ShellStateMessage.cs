using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace Console_MVVMTesting.Messages
{
    public enum ShellState
    {
        BusyOn,
        BusyOff,
        VersionOn,
        VersionOff,
        MyControlOn,
        MyControlOff,
        DateTimeOn,
        DateTimeOff,
        SettingsOn,
        SettingsOff,
    }

    public sealed class ShellStateMessage : AsyncRequestMessage<ShellState>
    {
        

        public ShellStateMessage(ShellState state)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                    $"ShellStateMessage::ShellStateMessage() " +
                    $"({this.GetHashCode():x8})");

            State = state;
        }


        public ShellState State { get; }
    }
}
