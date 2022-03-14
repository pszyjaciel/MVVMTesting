using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private readonly EastTesterViewModel _eastTesterViewModel;
        private const string consoleColor = "DGREEN";

        public ConnectionItem _connectionItem;
        private const string _connectionItem_Name = "LC-connection";
        private const int _connectionItem_Port = 42026;
        private const string _connectionItem_Host = "127.0.0.1";


        private ConcurrentQueue<string> commandQueue;
        private IPAddress _ipAddress = null;

        //private CancellationTokenSource _cancellationTokenSource1;
        private bool _ctsDisposed1 = false;
        private bool _closingDown = false;

        private AutoResetEvent ConnectedEvent = new AutoResetEvent(false);
        private AutoResetEvent ResponseReceivedEvent = new AutoResetEvent(false);
        private AutoResetEvent CommandInQueueEvent = new AutoResetEvent(false);
        private AutoResetEvent MessageHandlerTerminatedEvent = new AutoResetEvent(false);

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        List<Socket> myListOfSockets;


        #endregion privates



        private Post _myLCSocketPrivateProperyName;
        public Post MyLCSocketPublicProperyName
        {
            get => _myLCSocketPrivateProperyName;
            set => SetProperty(ref _myLCSocketPrivateProperyName, value);
        }



        public bool IsConnected(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"LCSocketViewModel::IsConnected(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

            bool result = (terminalSocket is not null) ? terminalSocket.Connected : false;
            return result;
        }


        #region Close
        /// <summary>
        /// Close method should be called when the Terminal connection is terminated.
        /// It closes the socket, and breaks out of the MessageHandler task.
        /// </summary>
        public void Close(Socket terminalSocket)    // pacz na _cancellationTokenSource1
        {
            _log.Log(consoleColor, $"LCSocketViewModel::Close(): socket: { terminalSocket.Handle}");
            if (!IsConnected(terminalSocket)) return;

            try
            {
                terminalSocket.Shutdown(SocketShutdown.Both);    // Close the socket
                terminalSocket.Close();
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::Close():  {se.SocketErrorCode}: {se.Message}");
            }
            catch (ObjectDisposedException ode)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::Close():  {ode.ObjectName}: {ode.Message}");
            }
            catch (Exception e)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::Close():  {e.HResult}: {e.Message}");
            }
            finally
            {
                //cts.Dispose();
                _ctsDisposed1 = true;
            }
            //_log.Log(consoleColor, $"LCSocketViewModel::Close(): ThreadId: {Thread.CurrentThread.ManagedThreadId}  End of method.");
        }
        #endregion Close


        #region IsHostnameValid
        /// <summary>
        /// Checks hostname for invalid characters. 
        /// Valid hostname characters are:
        /// A-Z, a-z, 0-9, minus(-) and period(.)
        /// 
        /// See https://stackoverflow.com/questions/106179/regular-expression-to-match-dns-hostname-or-ip-address
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        /// works in a new first-level Thread
        private bool IsHostnameValid(string hostname)
        {
            //_log.Log(consoleColor, $"LCSocketViewModel::IsHostnameValid(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

            bool valid = true;

            string ValidIpAddressRegex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            string ValidHostnameRegex = @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$";

            Regex rgxIpAdress = new Regex(ValidIpAddressRegex);
            Regex rgxHost = new Regex(ValidHostnameRegex);

            valid = rgxIpAdress.IsMatch(hostname);
            valid |= rgxHost.IsMatch(hostname);
            if (valid)
            {
                if (Int32.TryParse(hostname, out int num))
                    valid = false;
            }
            return valid;
        }
        #endregion IsHostnameValid


        #region ResolveIp
        private IPAddress ResolveIp(string host)
        {
            //_log.Log(consoleColor, $"LCSocketViewModel::ResolveIp(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");
            IPAddress ip = null;

            if (IsHostnameValid(host))
            {
                if (IPAddress.TryParse(host, out IPAddress ipAddress) == false)
                {
                    // Resolve IP from hostname
                    IPAddress[] IPs = Dns.GetHostEntry(host).AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray(); // Keep only IPv4 in IPs array
                    foreach (var ipAddr in IPs)
                    {
                        if (ipAddr.ToString().StartsWith("169.254") == false) // Get rid of internal Windows IPs
                        {
                            ip = ipAddr;
                            break;
                        }
                    }
                    if (ip is null)
                    {
                        ip = IPAddress.Parse(host);
                    }
                }
                else
                    ip = ipAddress;
            }

            return ip;
        }
        #endregion ResolveIp


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
            _log.Log("LCSocketViewModel::ParseOutputData()");

            string parsedData;
            parsedData = String.Format("{0}\r\n", outData);
            return parsedData;
        }
        #endregion ParseOutputData


        #region ConnectCallback
        private void ConnectCallback(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            //_log.Log(consoleColor, $"LCSocketViewModel::ConnectCallback(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            try
            {
                // Retrieve the socket from the state object.  
                Socket terminalSocket = (Socket)ar.AsyncState;

                // Complete the connection. Ends a pending asynchronous connection request.
                terminalSocket.EndConnect(ar); // throws an exception if connecting fails
                                               // Exception thrown: 'System.Net.Internals.SocketExceptionFactory.ExtendedSocketException' in System.Private.CoreLib.dll

                _connectionItem.ConnectTime = DateTime.Now;
                _log.Log(consoleColor, $"LCSocketViewModel::ConnectCallback(): socket: { terminalSocket.Handle} " +
                    $"connected to {terminalSocket.LocalEndPoint}");
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::ConnectCallback(): {se.SocketErrorCode} : {se.Message}");
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::ConnectCallback(): {ex.Message}");
            }
            // Signal that the connection has been made.  (or not)
            connectDone.Set();
            //_log.Log(consoleColor, $"LCSocketViewModel::ConnectCallback(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method.");
        }
        #endregion ConnectCallback


        #region MyConnectAsync
        private async Task<bool> MyConnectAsync(EndPoint remoteEP, Socket terminalSocket)
        {
            _log.Log(consoleColor, $"LCSocketViewModel::MyConnectAsync(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            IAsyncResult result = null;
            try
            {
                result = terminalSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), terminalSocket);

            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::MyConnectAsync(): {ex.Message}");
                return false;
            }

            // waiting for complition
            while (!result.IsCompleted)
            {
                await Task.Yield();
            }

            _log.Log(consoleColor, $"LCSocketViewModel::MyConnectAsync(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method.");
            return (result is not null) ? result.IsCompleted : false;
        }
        #endregion MyConnectAsync


        #region ConnectToSocket
        public bool ConnectToSocket(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"LCSocketViewModel::ConnectToSocket(): socket: {terminalSocket.Handle}, " +
                $"ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            Task MyTask = Task.Run(async () =>
            {
                int connectTry = 3;    // how many try to connect?
                _ipAddress = ResolveIp(_connectionItem.Host);
                EndPoint remoteEP = new IPEndPoint(_ipAddress, _connectionItem.Port);
                do
                {
                    await this.MyConnectAsync(remoteEP, terminalSocket);
                    connectDone.WaitOne();

                    if (IsConnected(terminalSocket) == false)
                    {
                        await Task.Delay(5000);
                        connectTry--;
                    }
                } while ((IsConnected(terminalSocket) == false) && (connectTry > 0));

                _log.Log(consoleColor, $"LCSocketViewModel::ConnectToSocket(): {terminalSocket.RemoteEndPoint} --> {terminalSocket.LocalEndPoint}");
            });
            MyTask.Wait();

            _log.Log(consoleColor, $"LCSocketViewModel::ConnectToSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method");
            return IsConnected(terminalSocket);
        }
        #endregion ConnectToSocket



        // To cancel a pending BeginReceive one can call the Close method.
        #region ReceiveCallBack
        private void ReceiveCallBack(IAsyncResult ar)
        {
            //string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            //_log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            // Retrieve the state object and the client socket from the asynchronous state object.  
            StateObject so = (StateObject)ar.AsyncState;
            Socket terminalSocket = so.workSocket;

            string response = "", tmpResponse = "";
            string heartbeat = "@\r\n";
            try
            {
                // the socket server returns 0x0d for a new line, and 0x0d 0x0a for end of message
                int receivedBytes = terminalSocket.EndReceive(ar); //   Ends a pending asynchronous read.
                _log.Log($"LCSocketViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}, receivedBytes: {receivedBytes}");

                // Connection error occured, as the server never returns 0 bytes
                if (receivedBytes == 0)
                {
                    _log.Log($"LCSocketViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}: It looks like a connection error!");
                    so.sb.Clear();   // clean the buffer

                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                    return;     // we will not close the connection!
                }

                if (receivedBytes > 0)
                {
                    // There might be more data, so store the data received so far.  
                    response = Encoding.ASCII.GetString(so.buffer, 0, receivedBytes);
                    tmpResponse = response.Replace(heartbeat, "");        // get rid of the heartbeat
                    if (tmpResponse.EndsWith("\r\n"))
                    {
                        so.sb.Append(tmpResponse);
                        receiveDone.Set();
                    }
                    else
                    {
                        //  Get the rest of the data.  (recursion!)
                        terminalSocket.BeginReceive(so.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallBack), so);
                    }
                }
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): {se.NativeErrorCode} : {se.SocketErrorCode} : {se.Message}");
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): {ex.Message}");
            }
            receiveDone.Set();
            //_log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion ReceiveCallBack



        #region ReceiveFromSocket
        public void ReceiveFromSocket(Socket terminalSocket, CancellationTokenSource cts)
        {
            _log.Log(consoleColor, $"LCSocketViewModel::ReceiveFromSocket(): " +
                $"socket: {terminalSocket.Handle} " +
                $"ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            if ((cts is null) || _ctsDisposed1 == true)
            {
                cts = new CancellationTokenSource();
                _ctsDisposed1 = false;
            }

            Task MyTask = Task.Run(() =>
           {
               try
               {
                   // Create the state object.  
                   StateObject state = new StateObject();
                   state.workSocket = terminalSocket;

                   // Begin receiving the data from the remote device. ReceiveCallBack() exits only when socket closing.
                   terminalSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
               }
               catch (Exception e)
               {
                   _log.Log(consoleColor, e.ToString());
               }

           });
            receiveDone.WaitOne();
            _log.Log(consoleColor, $"LCSocketViewModel::ReceiveFromSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method");
        }
        #endregion ReceiveFromSocket



        #region SendCallBack
        private void SendCallBack(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            _log.Log(consoleColor, $"LCSocketViewModel::SendCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            // Retrieve the socket from the state object.  
            Socket terminalSocket = (Socket)ar.AsyncState;
            _log.Log(consoleColor, $"LCSocketViewModel::SendCallBack: socket: {terminalSocket.Handle}");

            try
            {
                // Complete sending the data to the remote device.  
                int bytesSend = terminalSocket.EndSend(ar);
                _log.Log(consoleColor, $"LCSocketViewModel::SendCallBack: bytesSend: {bytesSend}");

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::SendCallBack: {ex.Message}");
            }
            _log.Log(consoleColor, $"LCSocketViewModel::SendCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion SendCallBack


        #region SendToSocket
        // BeginSend() Sends data asynchronously to a connected System.Net.Sockets.Socket.
        // works in a second level new Thread!
        public void SendToSocket(Socket terminalSocket, string text)   // hujowata nazwa
        {
            _log.Log(consoleColor, $"LCSocketViewModel::SendToSocket(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            Task MyTask = Task.Run(() =>
            {
                try
                {
                    if (terminalSocket is not null)
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes(text);
                        terminalSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallBack), terminalSocket);
                    }
                }
                catch (SocketException se)
                {
                    _log.Log(consoleColor, $"LCSocketViewModel::SendToSocket(): {se.SocketErrorCode}: {se.Message}");
                }
                sendDone.WaitOne();
            });

            _log.Log(consoleColor, $"LCSocketViewModel::SendToSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion SendToSocket


        #region RunInitCommandMessage2
        private void RunInitCommandMessage2()
        {
            //_log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            CancellationTokenSource myCancellationTokenSource1 = new CancellationTokenSource();
            CancellationTokenSource myCancellationTokenSource2 = new CancellationTokenSource();
            CancellationTokenSource myCancellationTokenSource3 = new CancellationTokenSource();
            CancellationTokenSource myCancellationTokenSource4 = new CancellationTokenSource();

            Task MyTask;

            Socket terminalSocket1 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (!IsConnected(terminalSocket1))
            {
                this.ConnectToSocket(terminalSocket1);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            Socket terminalSocket2 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (!IsConnected(terminalSocket2))
            {
                this.ConnectToSocket(terminalSocket2);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            Socket terminalSocket3 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (!IsConnected(terminalSocket3))
            {
                this.ConnectToSocket(terminalSocket3);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            Socket terminalSocket4 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (!IsConnected(terminalSocket4))
            {
                this.ConnectToSocket(terminalSocket4);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            ///// receiving /////
            if (IsConnected(terminalSocket1))
            {
                this.ReceiveFromSocket(terminalSocket1, myCancellationTokenSource1);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            if (IsConnected(terminalSocket2))
            {
                this.ReceiveFromSocket(terminalSocket2, myCancellationTokenSource2);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            if (IsConnected(terminalSocket3))
            {
                this.ReceiveFromSocket(terminalSocket3, myCancellationTokenSource3);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            if (IsConnected(terminalSocket4))
            {
                this.ReceiveFromSocket(terminalSocket4, myCancellationTokenSource4);
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }


            ///// sending /////
            if (IsConnected(terminalSocket1))
            {
                this.SendToSocket(terminalSocket1, ParseOutputData("L10"));
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            if (IsConnected(terminalSocket2))
            {
                this.SendToSocket(terminalSocket2, ParseOutputData("L11"));
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            if (IsConnected(terminalSocket3))
            {
                this.SendToSocket(terminalSocket3, ParseOutputData("L12"));
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }

            if (IsConnected(terminalSocket4))
            {
                this.SendToSocket(terminalSocket4, ParseOutputData("L13"));
                MyTask = Task.Delay(250);
                MyTask.Wait();
            }


            // now we want to close the open sockets
            _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): waiting delay before we close all sockets..");
            MyTask = Task.Delay(3000);
            MyTask.Wait();

            Task MyClosingTask = Task.Run(() =>
               {
                   this.Close(terminalSocket1);
                   this.Close(terminalSocket2);
                   this.Close(terminalSocket3);
                   this.Close(terminalSocket4);

                   _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket1.Handle} connected: {IsConnected(terminalSocket1)}");
                   _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket2.Handle} connected: {IsConnected(terminalSocket2)}");
                   _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket3.Handle} connected: {IsConnected(terminalSocket3)}");
                   _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket4.Handle} connected: {IsConnected(terminalSocket4)}");
               });

            MyClosingTask.Wait();   // wait before exit

            //_log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): End of method.");
        }
        #endregion RunInitCommandMessage2






        ///////////// INITIALIZING SOCKETS /////////////////
        private List<Socket> GetListOfAvailableSockets()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::GetListOfAvailableSockets(): Start of method");

            List<Socket> ls = new List<Socket>();
            ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));

            _log.Log(consoleColor, $"LCSocketViewModel::GetListOfAvailableSockets(): End of method");
            return ls;
        }

        // async doesn't work well with Parallel.ForEach: https://stackoverflow.com/a/23139769/7036047
        // The whole idea behind Parallel.ForEach() is that you have a set of threads and each thread processes part of the collection.
        // This doesn't work with async-await, where you want to release the thread for the duration of the async call.
        private void RunInitCommandMessage()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            List<Socket> myListOfAvailableSockets = GetListOfAvailableSockets();
            foreach (Socket myAvailableSocket in myListOfAvailableSockets)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): socket: {myAvailableSocket.Handle}, " +
                    $"connected: {myAvailableSocket.Connected}");
            }

            bool rs = false;
            myListOfSockets = new List<Socket>();
            Parallel.ForEach(myListOfAvailableSockets, (mySocket) =>
            {
                if (!IsConnected(mySocket))
                {
                    rs = this.ConnectToSocket(mySocket);
                }
                if (rs)
                {
                    myListOfSockets.Add(mySocket);
                }
                rs = false;
            });

            _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): myListOfSockets.Count: {myListOfSockets.Count}");
            foreach (Socket myConnectedSocket in myListOfSockets)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): socket: {myConnectedSocket.Handle}, " +
                    $"connected: {myConnectedSocket.Connected}");
            }

            _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): End of method  ({this.GetHashCode():x8})");
        }



        ////////////////// CLOSING SOCKETS /////////////////
        private void RunCloseCommandMessage()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::RunCloseCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            List<Socket> myInitializedListOfSockets = new List<Socket>();
            Parallel.ForEach(myListOfSockets, (mySocket) =>
            {
                this.Close(mySocket);
                _log.Log(consoleColor, $"LCSocketViewModel::CloseAllSocketsParallel(): mySocket {mySocket.Handle}: {mySocket.Connected}");
            });

            _log.Log(consoleColor, $"LCSocketViewModel::RunCloseCommandMessage(): End of method  ({this.GetHashCode():x8})");
        }


        #region Constructor
        public LCSocketViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;

            _connectionItem = new ConnectionItem();
            _connectionItem.Name = _connectionItem_Name;
            _connectionItem.Port = _connectionItem_Port;
            _connectionItem.Host = _connectionItem_Host;

            _ipAddress = new IPAddress(0);
            commandQueue = new ConcurrentQueue<string>();


            // listen for the command in ProductionViewModel
            //_messenger.Register<LCInitMessage>(this, (r, m) => { RunInitCommandMessage(); });
            //_messenger.Register<LCCloseMessage>(this, (r, m) => { RunCloseCommandMessage(); });

            ///// PropertyChangedMessage /////
            Post myLCSocketPrivateProperyNameOld = new Post
            {
                Updated = false,
                Title = "LCSocketOldTitle2",
                Thumbnail = "LCSocketOldThumbnail2",
                SelfText = "Some old LCSocket text2"
            };

            Post myLCSocketPrivateProperyNameNew = new Post
            {
                Updated = true,
                Title = "LCSocketNewTitle",
                Thumbnail = "LCSocketNewThumbnail",
                SelfText = "Some new LCSocket text"
            };


            //C:\Users\pak\Source\Repos\MVVM-Samples-master\samples\MvvmSampleUwp.sln

            _messenger.Send(new PropertyChangedPostMessage(this, nameof(MyLCSocketPublicProperyName),
                myLCSocketPrivateProperyNameOld, myLCSocketPrivateProperyNameNew));

            LCSocketStateMessage lcssm = new LCSocketStateMessage(LCSocketStatusEnum.Connected);
            //_messenger.Send(this, lcssm);     // wywala

            //TMessage Send<TMessage, TToken>(TMessage message, TToken token)
            //    where TMessage : class
            //    where TToken : IEquatable<TToken>;

            //_log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel() XamlLCSocketViewModel.GetHashCode(): {XamlLCSocketViewModel.GetHashCode()}");





            // LoggedInUserRequestMessage is requested from the ProductionViewModel
            _messenger.Register<LCSocketViewModel, LoggedInUserRequestMessage>(this, (r, m) =>
            {
                m.Reply(r.GetLCSocketUser());
            });


            MyUser myUser = new MyUser { MyUserName = "LCSocketUserName"};
            // Send a message from some other module
            _messenger.Send(new LoggedInUserChangedMessage(myUser));


            Task MyTask = Task.Run(MyLongRunningTask);

            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }

        private async Task MyLongRunningTask()
        {
            while (true)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::MyRunningTask()");
                await Task.Delay(2000);
            }
        }

        private MyUser GetLCSocketUser()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::GetLCSocketUser()");
            return new MyUser("MyLCSocketUser");
        }
        #endregion Constructor

    }
}