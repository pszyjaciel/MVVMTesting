using System;
using System.Net.Sockets;
using System.Text;

namespace Console_MVVMTesting.ViewModels        // to be moved some other place
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
        public int dataRecieved = 0;
        public DateTime TimeStamp;                      //timestamp of data
    }
}
