using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System.Net.Sockets;

namespace Console_MVVMTesting.Messages
{
    public enum LCSocketStatusEnum
    {
        Initialized,
        Connected,
        Disconnected,
        Success,
        Error,
    }

    public sealed class LCSocketStateMessage : AsyncRequestMessage<LCSocketStatusEnum>
    {
        public LCSocketStatusEnum LCStatus { get; }

        public LCSocketStateMessage(LCSocketStatusEnum myStatus)
        {
            System.Diagnostics.Debug.WriteLine($"LCSocketStateMessage::LCSocketStateMessage()");
            LCStatus = myStatus;
        }

    }
}
