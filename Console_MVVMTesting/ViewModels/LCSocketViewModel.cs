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
        private ManualResetEvent disconnectDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        private List<Socket> _myListOfSockets;

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
            _log.Log(consoleColor, "LCSocketViewModel::ParseOutputData()");

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
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            _log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            // Retrieve the state object and the client socket from the asynchronous state object.  
            StateObject so = (StateObject)ar.AsyncState;
            Socket terminalSocket = so.workSocket;

            string response = "", tmpResponse = "";
            string heartbeat = "@\r\n";
            try
            {
                // the socket server returns 0x0d for a new line, and 0x0d 0x0a for end of message
                int receivedBytes = terminalSocket.EndReceive(ar); //   Ends a pending asynchronous read.
                _log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}, receivedBytes: {receivedBytes}");

                // Connection error occured, as the server never returns 0 bytes
                //if (receivedBytes == 0)
                //{
                //    _log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}: It looks like a connection error!");
                //    so.sb.Clear();   // clean the buffer

                //    // Signal that all bytes have been received.  
                //    receiveDone.Set();
                //    return;     // we will not close the connection!
                //}

                if (receivedBytes > 0)
                {
                    // There might be more data, so store the data received so far.  
                    response = Encoding.ASCII.GetString(so.buffer, 0, receivedBytes);
                    //tmpResponse = response.Replace(heartbeat, "");        // get rid of the heartbeat
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
            finally
            {
                receiveDone.Set();
            }
            _log.Log(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion ReceiveCallBack


        // dopuszczalne tylko jedno wywolanie typu odpal i zapomnij
        #region ReceiveFromSocket
        public string ReceiveFromSocket(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"LCSocketViewModel::ReceiveFromSocket(): " +
                $"socket: {terminalSocket.Handle} " +
                $"ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = terminalSocket;
            state.socketConnected = true;   // musi ma byc

            try
            {
                // Begin receiving the data from the remote device. ReceiveCallBack() exits only when socket closing.
                terminalSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
            }
            catch (Exception e)
            {
                _log.Log(consoleColor, e.ToString());
            }

            receiveDone.WaitOne();

            _log.Log(consoleColor, $"LCSocketViewModel::ReceiveFromSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method");
            return state.sb.ToString();
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
        public string SendToSocket(Socket terminalSocket, string text)
        {
            if (terminalSocket == null) return String.Empty;
            if (IsConnected(terminalSocket) is false) return String.Empty;

            _log.Log(consoleColor, $"LCSocketViewModel::SendToSocket(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            //MyUtils.DisplayStringInBytes(text);

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
                _log.Log(consoleColor, $"LCSocketViewModel::SendToSocket(): {se.NativeErrorCode} : {se.SocketErrorCode}: {se.Message}");
            }
            sendDone.WaitOne();

            // string response = this.ReceiveFromSocket(terminalSocket);

            _log.Log(consoleColor, $"LCSocketViewModel::SendToSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
            return "";
        }
        #endregion SendToSocket





        #region DisconnectCallback
        private void DisconnectCallback(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            _log.Log(consoleColor, "LCSocketViewModel::DisconnectCallback(): Start of method");

            // Complete the disconnect request.
            Socket client = (Socket)ar.AsyncState;
            _log.Log(consoleColor, $"LCSocketViewModel::DisconnectCallback(): socket: {client.Handle}");

            try
            {
                client.EndDisconnect(ar);  // Ends a pending asynchronous disconnect request.
                                           //client.Close();     // nie za bardzo..
                _log.Log(consoleColor, $"LCSocketViewModel::DisconnectCallback: The terminal is disconnected!");
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::DisconnectCallback: {se.ErrorCode}: {se.Message}");
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::DisconnectCallback: {ex.Message}");
            }
            // Signal that the disconnect is complete.
            disconnectDone.Set();
            _log.Log(consoleColor, "LCSocketViewModel::DisconnectCallback(): End of method");
        }
        #endregion

        #region Disconnect
        // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.begindisconnect?view=net-6.0
        public bool Disconnect(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"LCSocketViewModel::Disconnect(): socket: {terminalSocket.Handle} - Start of method");
            bool result = false;

            if (terminalSocket.Connected)
            {
                try
                {
                    //this.Close(terminalSocket, cts);

                    // Release the socket.
                    // To ensure that all data is sent and received before the socket is closed, you should call Shutdown before calling the Disconnect method.
                    terminalSocket.Shutdown(SocketShutdown.Both);   // Make sure to do this

                    // Begins an asynchronous request to disconnect from a remote endpoint.
                    // Disconnect() is usually only used when you plan to reuse the same socket.
                    // It will block until the TIME_WAIT period expires (several minutes, it´s a OS-wide setting).
                    // So usually you do a Shutdown() followed by a Close() on both sides.
                    // public IAsyncResult BeginDisconnect(bool reuseSocket, AsyncCallback? callback, object? state)
                    terminalSocket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), terminalSocket);

                    // Wait for the disconnect to complete.
                    disconnectDone.WaitOne();
                    if (terminalSocket.Connected)
                    {
                        _log.Log(consoleColor, "LCSocketViewModel::Disconnect(): We're still connected");
                        result = false;
                    }
                    else
                    {
                        _log.Log(consoleColor, "LCSocketViewModel::Disconnect(): We're disconnected");
                        result = true;
                    }
                }
                catch (SocketException se)
                {
                    _log.Log(consoleColor, $"LCSocketViewModel::Disconnect(): {se.SocketErrorCode} : {se.Message}");
                    result = false;
                }
                catch (Exception ex)
                {
                    _log.Log(consoleColor, $"LCSocketViewModel::Disconnect(): {ex.Message}");
                    result = false;
                }

            }
            _log.Log(consoleColor, $"LCSocketViewModel::Disconnect(): End of method: result: {result}");
            return result;
        }
        #endregion



        ////////////////// CLOSING SOCKETS /////////////////
        private void RunCloseCommandMessage()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::RunCloseCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            List<Socket> myInitializedListOfSockets = new List<Socket>();
            Parallel.ForEach(_myListOfSockets, (mySocket) =>
            {
                this.Close(mySocket);
                _log.Log(consoleColor, $"LCSocketViewModel::CloseAllSocketsParallel(): mySocket {mySocket.Handle}: {mySocket.Connected}");
            });

            _log.Log(consoleColor, $"LCSocketViewModel::RunCloseCommandMessage(): End of method  ({this.GetHashCode():x8})");
        }

        private MyUser GetLCSocketUser()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::GetLCSocketUser()");
            return new MyUser("MyLCSocketUser");
        }



        #region RunLCInitCommandMessage
        ///////////// INITIALIZING SOCKETS /////////////////
        // async doesn't work well with Parallel.ForEach: https://stackoverflow.com/a/23139769/7036047
        // The whole idea behind Parallel.ForEach() is that you have a set of threads and each thread processes part of the collection.
        // This doesn't work with async-await, where you want to release the thread for the duration of the async call.
        private bool RunLCInitCommandMessage()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            bool initResult = true;

            List<Socket> myListOfAvailableSockets = new List<Socket>();
            myListOfAvailableSockets.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            //myListOfAvailableSockets.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            //myListOfAvailableSockets.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            //myListOfAvailableSockets.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));

            int numberOfAvailableSockets = myListOfAvailableSockets.Count;
            int numberOfInitializedSockets = 0;
            _myListOfSockets = new List<Socket>();

            // obs: parallel call:
            Parallel.ForEach(myListOfAvailableSockets, (mySocket) =>
            {
                bool rs = false;
                if (!IsConnected(mySocket))
                {
                    rs = this.ConnectToSocket(mySocket);
                }
                if (rs)
                {
                    ///// receiving the hello message /////
                    string response = this.ReceiveFromSocket(mySocket);
                    _log.Log(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): Socket { mySocket.Handle} got response: {response}");

                    _myListOfSockets.Add(mySocket);
                    numberOfInitializedSockets++;
                }
            });

            _log.Log(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): _myListOfSockets.Count: {_myListOfSockets.Count}");


            if (numberOfInitializedSockets != numberOfAvailableSockets)
            {
                _log.Log(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): not all sockets have been initialized");
                initResult = false;
            }

            // we are ready with initialize
            _log.Log(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): End of method with initResult: {initResult}  ({this.GetHashCode():x8})");
            return initResult;
        }
        #endregion RunLCInitCommandMessage


        #region LCSocketInitAsync
        //this method should not return any value until all sockets have been initialized
        private async Task<LCSocketStateMessage> LCSocketInitAsync()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketInitAsync() - start of method");

            //public Dictionary<IntPtr, Tuple<int, string>> SocketInitDict { get; set; }     // socket_handle, error_code, error_description
            Tuple<int, string> myTyple = new Tuple<int, string>(0, "");
            Dictionary<IntPtr, Tuple<int, string>> myDictionary = new Dictionary<IntPtr, Tuple<int, string>>();

            LCSocketStateMessage lcssm = new LCSocketStateMessage();
            lcssm.SocketInitDict = myDictionary;

            bool rs = await Task.Run(RunLCInitCommandMessage);
            lcssm.LCErrorNumber = rs ? 0 : -1;      // error number can expand
            lcssm.MyStateName = "LCSocketInitAsync";
            lcssm.lcStatus = rs ? LCStatus.Success : LCStatus.Error;

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

            if (_myListOfSockets == null)
                return new LCSocketStateMessage { LCErrorNumber = -1 };

            bool shutDownResult = true;
            int numberOfDisconnectedSockets = 0;

            // obs: parallel call
            Parallel.ForEach(_myListOfSockets, (mySocket) =>
            {
                bool rs = false;
                rs = this.Disconnect(mySocket);
                _log.Log(consoleColor, $"LCSocketViewModel::RunLCShutdownCommand(): mySocket {mySocket.Handle}: {mySocket.Connected}");
                if (rs)
                {
                    numberOfDisconnectedSockets++;
                }
            });

            _log.Log(consoleColor, $"LCSocketViewModel::RunLCShutdownCommand(): numberOfDisconnectedSockets: {numberOfDisconnectedSockets}, _myListOfSockets.Count: {_myListOfSockets.Count}");
            if (numberOfDisconnectedSockets != _myListOfSockets.Count)
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

            if (_myListOfSockets.Count == 0)
            {
                MySocketDict.Add((IntPtr)0, new Tuple<string, double, int>("", 0.0, -1));
            }

            foreach (Socket terminalSocket in _myListOfSockets)
            {
                string myResponseL10 = this.SendToSocket(terminalSocket, ParseOutputData("L10"));
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

            if (_myListOfSockets == null)
            {
                return new LCSocketStateMessage { LCErrorNumber = -1 };
            }

            Tuple<UInt16, UInt16> myTyple = new Tuple<UInt16, UInt16>(0, 0);
            Dictionary<IntPtr, Tuple<UInt16, UInt16>> myDictionary = new Dictionary<IntPtr, Tuple<UInt16, UInt16>>();

            //public Dictionary<IntPtr, Tuple<UInt16, UInt16>> BatteryStatusDict { get; set; }

            LCSocketStateMessage lcssm = new LCSocketStateMessage();

            //Tuple<int, string> myTuple = new Tuple<int, string>(-1, "");
            //Dictionary<IntPtr, Tuple<int, string>> myDictionary = new Dictionary<IntPtr, Tuple<int, string>>(_myListOfSockets[0].Handle, myTuple);

            lcssm.BatteryStatusDict = myDictionary;

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

            _connectionItem = new ConnectionItem();
            _connectionItem.Name = _connectionItem_Name;
            _connectionItem.Port = _connectionItem_Port;
            _connectionItem.Host = _connectionItem_Host;

            _ipAddress = new IPAddress(0);
            commandQueue = new ConcurrentQueue<string>();


            //LoggedInUserRequestMessage is requested from the ProductionViewModel
            //_messenger.Register<LCSocketViewModel, LoggedInUserRequestMessage>(this, (r, m) =>
            //{
            //    m.Reply(r.GetLCSocketUser());
            //});

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