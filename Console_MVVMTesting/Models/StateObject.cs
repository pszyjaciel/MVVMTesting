using System;
using System.Net.Sockets;
using System.Text;

namespace Console_MVVMTesting.Models
{
    public class StateObject
    {
        public bool socketConnected = false;
        public const int BufferSize = 1024;           //size of receive buffer
        public byte[] buffer = new byte[BufferSize];  //receive buffer
        public int dataSize = 0;                      //data size to be received
        public bool dataSizeReceived = false;         //received data size?

        // Client socket.  
        public Socket workSocket = null;

        // Received data string.  
        public StringBuilder sb = new StringBuilder();
        public int dataReceived = 0;
        public DateTime TimeStamp;                      //timestamp of data


        public StateObject()
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] StateObject::StateObject()  ({this.GetHashCode():x8})");
        }

        public StateObject(bool socketConnected, byte[] buffer, int dataSize, bool dataSizeReceived, Socket workSocket, StringBuilder sb, int dataReceived, DateTime timeStamp)
        {
            this.socketConnected = socketConnected;
            this.buffer = buffer;
            this.dataSize = dataSize;
            this.dataSizeReceived = dataSizeReceived;
            this.workSocket = workSocket;
            this.sb = sb;
            this.dataReceived = dataReceived;
            TimeStamp = timeStamp;
        }
    }
}
