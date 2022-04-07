using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static PInvoke.User32;


// (!)messages: https://social.technet.microsoft.com/wiki/contents/articles/30939.wpf-change-tracking.aspx
// https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-an-asynchronous-client-socket
// https://docs.microsoft.com/en-us/dotnet/api/system.asynccallback?redirectedfrom=MSDN&view=net-6.0


namespace Console_MVVMTesting.ViewModels
{
    public class LCSocketViewModel : ObservableObject
    {
        #region privates
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;

        private const string consoleColor = "LMAGENTA";


        private const string _connectionItem_Name = "LC-connection";
        private const int _connectionItem_Port = 42026;
        private const string _connectionItem_Host = "127.0.0.1";

        LCSocket lcs;

        private ConcurrentQueue<string> commandQueue;
    

        //private CancellationTokenSource _cancellationTokenSource1;
        private bool _ctsDisposed1 = false;
        private bool _closingDown = false;

        private AutoResetEvent ConnectedEvent = new AutoResetEvent(false);
        private AutoResetEvent ResponseReceivedEvent = new AutoResetEvent(false);
        private AutoResetEvent CommandInQueueEvent = new AutoResetEvent(false);
        private AutoResetEvent MessageHandlerTerminatedEvent = new AutoResetEvent(false);


        #endregion privates



        private Post _myLCSocketPrivateProperyName;
        public Post MyLCSocketPublicProperyName
        {
            get => _myLCSocketPrivateProperyName;
            set => SetProperty(ref _myLCSocketPrivateProperyName, value);
        }


 

 

        #region ParseResponseData
        /// <summary>
        /// Remove trailing '\r', '\n' and '@'
        /// </summary>
        private string ParseResponseData(ref String data)
        {
            //_log.Log(consoleColor, $"LCSocketViewModel::ParseResponseData()");
            //_log.Log( $"LCSocketViewModel::ParseResponseData(): data: {data}");
            //MyUtils.DisplayStringInBytes(data);

            //string parsedData = data;
            //40 0d 0a
            //if (data.Length == 3)
            //{
            //    if (((data[0] == 0x40) && (data[1] == 0x0a) && (data[2] == 0x0d))
            //        || ((data[0] == 0x40) && (data[1] == 0x0d) && (data[2] == 0x0a)))
            //    {
            //        parsedData = "[heartbeat]\n";
            //    }
            //}

            string parsedData = data;
            string tmp;
            do
            {
                tmp = parsedData;
                parsedData = parsedData.TrimEnd(new char[] { '\r', '\n', '@' });
                //parsedData = parsedData.TrimEnd(new char[] { '@' });
            }
            while (parsedData != tmp);

            // this @ (0x40) can be treated as a heartbeat
            //_log.Log(consoleColor, $"SocketViewModel::ParseResponseData(): parsedData:\n{parsedData}\n");
            //MyUtils.DisplayStringInBytes(parsedData);
            data = parsedData;
            return data;
        }
        #endregion ParseResponseData


        #region ParseOutputData
        // adds '\r\n' to the end of string data
        private string ParseOutputData(string outData)
        {
            _log.Log(consoleColor, "LCSocketViewModel::ParseOutputData()");

            string parsedData;
            parsedData = String.Format("{0}\r\n", outData);
            return parsedData;
        }
        #endregion ParseOutputData


     




    




        ////////////////// CLOSING SOCKETS /////////////////
        private void RunCloseCommandMessage()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::RunCloseCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            List<Socket> myInitializedListOfSockets = new List<Socket>();
            Parallel.ForEach(lcs.myListOfSockets, (mySocket) =>
            {
                lcs.Close(mySocket);
                _log.Log(consoleColor, $"LCSocketViewModel::CloseAllSocketsParallel(): mySocket {mySocket.Handle}: {mySocket.Connected}");
            });

            _log.Log(consoleColor, $"LCSocketViewModel::RunCloseCommandMessage(): End of method  ({this.GetHashCode():x8})");
        }

        private MyUser GetLCSocketUser()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::GetLCSocketUser()");
            return new MyUser("MyLCSocketUser");
        }



        #region LCSocketInitAsync
        //this method should not return any value until all sockets have been initialized
        private async Task<LCSocketStateMessage> LCSocketInitAsync()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketInitAsync() - start of method");

            LCSocketStateMessage lcssm = await Task.Run(lcs.RunLCInitCommand);
            
            //return new LCSocketStateMessage { MyStateName = "LCSocketViewModel", lcStatus = rs ? LCStatus.Success : LCStatus.Error };
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketInitAsync() - end of method");
            return lcssm;
        }
        #endregion LCSocketInitAsync





        #region RunLCShutdownCommand
        ////////////////// CLOSING SOCKETS /////////////////
        //private void RunLCShutdownCommand()
        private LCSocketStateMessage RunLCShutdownCommand()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::RunLCShutdownCommand(): Start of method  ({this.GetHashCode():x8})");

            if (lcs.myListOfSockets == null)
                return new LCSocketStateMessage { LCErrorNumber = -1 };

            bool shutDownResult = true;
            int numberOfDisconnectedSockets = 0;

            // obs: parallel call
            Parallel.ForEach(lcs.myListOfSockets, (mySocket) =>
            {
                bool rs = false;
                rs = lcs.Disconnect(mySocket);
                _log.Log(consoleColor, $"LCSocketViewModel::RunLCShutdownCommand(): mySocket {mySocket.Handle}: {mySocket.Connected}");
                if (rs)
                {
                    numberOfDisconnectedSockets++;
                }
            });

            _log.Log(consoleColor, $"LCSocketViewModel::RunLCShutdownCommand(): numberOfDisconnectedSockets: {numberOfDisconnectedSockets}, " +
                $"myListOfSockets.Count: {lcs.myListOfSockets.Count}");
            if (numberOfDisconnectedSockets != lcs.myListOfSockets.Count)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::RunLCShutdownCommand(): not all sockets have been shuted down properly.");
                shutDownResult = false;
            }

            LCSocketStateMessage lcssm = new LCSocketStateMessage();
            lcssm.LCErrorNumber = shutDownResult ? 0 : -1;      // error number can expand
            lcssm.MyStateName = "LCSocketViewModel";
            lcssm.lcStatus = shutDownResult ? LCStatus.Success : LCStatus.Error;

            _log.Log(consoleColor, $"LCSocketViewModel::RunLCShutdownCommand(): End of method  ({this.GetHashCode():x8})");
            return lcssm;
        }
        #endregion RunLCShutdownCommand




        #region LCSocketCheckPowerSupplyCommand
        private string ParsePowerInputString(string response, string myParameterIamLookingFor)
        {
            _log.Log(consoleColor, $"LCSocketViewModel::ParsePowerInputString(): Start of method ");

            //string[] stringSeparators = new string[] { "\r\n" };
            //string[] lines = response.Split(stringSeparators, StringSplitOptions.None);
            string powerInputResult = string.Empty;
            string[] responseLines = response.Split('\r', StringSplitOptions.None);

            foreach (string myLine in responseLines)
            {
                if (!myLine.Contains(myParameterIamLookingFor))
                {
                    continue;       // take the next line
                }
                powerInputResult = myLine.Substring(myLine.IndexOf(':') + 1).Trim();
                break;
            }

            _log.Log(consoleColor, $"LCSocketViewModel::ParsePowerInputString(): End of method ");
            return powerInputResult;
        }

        // L10
        private LCSocketStateMessage LCSocketCheckPowerSupplyCommand()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketCheckPowerSupplyCommand(): Start of method  ({this.GetHashCode():x8})");

            double myVoltage = 0.0;
            //CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            CultureInfo cultureInfo = new("en-US");
            NumberStyles styles = NumberStyles.Number;

            int LCErrorCode = 0;
            LCSocketStateMessage lcssm = new();
            lcssm.MyStateName = "LCSocketCheckPowerSupplyCommand";
            Dictionary<IntPtr, Tuple<string, double, int>> MySocketDict = new();

            if (lcs.myListOfSockets.Count == 0)
            {
                MySocketDict.Add((IntPtr)0, new Tuple<string, double, int>("", 0.0, -1));
            }

            foreach (Socket terminalSocket in lcs.myListOfSockets)
            {
                string myResponseL10 = lcs.SendToSocket(terminalSocket, ParseOutputData("L10"));
                string myResponsePowerInput = this.ParsePowerInputString(myResponseL10, "Power Input");
                string myResponseACIn = this.ParsePowerInputString(myResponseL10, "AC In");
                string[] responseLines = myResponseACIn.Split(' ', StringSplitOptions.None);

                foreach (string myVoltageLine in responseLines)
                {
                    if (!myVoltageLine.Contains('V'))
                        continue;

                    bool isDouble = double.TryParse(myVoltageLine.Trim('V'), styles, cultureInfo, out myVoltage);
                    if (isDouble)
                        LCErrorCode = (myVoltage < 240 && myVoltage > 200) ? 0 : -2;
                    else
                        LCErrorCode = -3;   // Could'nt parse the voltage value
                }

                Tuple<string, double, int> MyTuple = new(myResponsePowerInput, myVoltage, LCErrorCode);
                MySocketDict.Add(terminalSocket.Handle, MyTuple);
            }

            lcssm.MySocket = MySocketDict;

            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketCheckPowerSupplyCommand(): End of method  ({this.GetHashCode():x8})");
            return lcssm;
        }
        #endregion LCSocketCheckPowerSupplyCommand


        #region LCSocketCheckBatteryCommand
        private LCSocketStateMessage LCSocketCheckBatteryCommand()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketCheckBatteryCommand(): Start of method  ({this.GetHashCode():x8})");

            //Socket mySocket = new Socket(lcs.ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //List<Socket> myListOfSockets = new List<Socket> { mySocket };
            //_log.Log(consoleColor, $"LCSocketViewModel::LCSocketCheckBatteryCommand(): myListOfSockets.Count: {myListOfSockets.Count}");

            Tuple<UInt16, UInt16> myTuple = new Tuple<UInt16, UInt16>(0xffff, 0xffff);
            LCSocketStateMessage lcssm = new LCSocketStateMessage();
            Dictionary<IntPtr, Tuple<UInt16, UInt16>> myBatteryStatusDict = new Dictionary<IntPtr, Tuple<ushort, ushort>>
            {
                //{ mySocket.Handle, myTuple }
            };

            lcssm.BatteryStatusDict = myBatteryStatusDict;

            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketCheckBatteryCommand(): End of method  ({this.GetHashCode():x8})");
            return lcssm;
        }
        #endregion LCSocketCheckBatteryCommand


        #region Constructor
        public LCSocketViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;

            lcs = new LCSocket(1);      // w nawiasie jest ilosc setuf


            _messenger.Register<LCSocketViewModel, LCSocketInitRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                // musi zwracac wszyskie podlonczone sokety do ProductionViewModel
                myMessenger.Reply(myReceiver.LCSocketInitAsync());
            });

            _messenger.Register<LCSocketViewModel, LCShutdownRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.RunLCShutdownCommand());       // pacz ShellViewModel::IsShuttingDown
            });

            _messenger.Register<LCSocketViewModel, LCSocketCheckPowerSupplyRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.LCSocketCheckPowerSupplyCommand());       // pacz ShellViewModel::IsShuttingDown
            });

            _messenger.Register<LCSocketViewModel, LCSocketCheckBatteryStatusRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.LCSocketCheckBatteryCommand());       // pacz ShellViewModel::IsShuttingDown
            });


            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }
        #endregion Constructor

    }
}