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
    public class TRSocketIPsViewModel : ObservableObject
    {
        #region privates
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;

        private readonly EastTesterViewModel _eastTesterViewModel;
        private const string consoleColor = "DGREEN";

        //public ConnectionItem _connectionItem;
        private List<ConnectionItem> _connectionItems;
        private List<Tuple<Socket, IPAddress, int>> _connectionItemList;

        private int _mySocketNativeErrorCode;
        //Dictionary<IntPtr, int> _mySocketErrorDict = new Dictionary<IntPtr, int>();
        Dictionary<IntPtr, SocketException> _mySocketErrorDict = new Dictionary<IntPtr, SocketException>();



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



        //private Post _myTRSocketPrivateProperyName;
        //public Post MyTRSocketPublicProperyName
        //{
        //    get => _myTRSocketPrivateProperyName;
        //    set => SetProperty(ref _myTRSocketPrivateProperyName, value);
        //}

        
        public SocketException GetLastError(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::GetLastError(): socket: {terminalSocket.Handle}, " +
                $"ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method");

            SocketException se = null;
            foreach (KeyValuePair<IntPtr, SocketException> item in _mySocketErrorDict)
            {
                if (item.Key == terminalSocket.Handle)
                {
                    se = item.Value;
                    _mySocketErrorDict.Remove(item.Key);
                    break;
                }
            }

            _log.Log(consoleColor, $"TRSocketIPsViewModel::GetLastError(): socket: {terminalSocket.Handle}, " +
                $"ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method");

            return se;
        }

        public bool IsConnected(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::IsConnected(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

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
            _log.Log(consoleColor, $"TRSocketIPsViewModel::Close(): socket: { terminalSocket.Handle}");
            if (!IsConnected(terminalSocket)) return;

            try
            {
                terminalSocket.Shutdown(SocketShutdown.Both);    // Close the socket
                terminalSocket.Close();
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::Close():  {se.SocketErrorCode}: {se.Message}");
            }
            catch (ObjectDisposedException ode)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::Close():  {ode.ObjectName}: {ode.Message}");
            }
            catch (Exception e)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::Close():  {e.HResult}: {e.Message}");
            }
            finally
            {
                //cts.Dispose();
                _ctsDisposed1 = true;
            }
            //_log.Log(consoleColor, $"TRSocketIPsViewModel::Close(): ThreadId: {Thread.CurrentThread.ManagedThreadId}  End of method.");
        }
        #endregion Close


        #region ResolveIp
        private IPAddress ResolveIp(string host2)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): host: {host2}, ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

            string host = "127.0.0.1";
            IPAddress[] IPs = null;
            try
            {
                // Resolve IP from hostname; Keep only IPv4 in IPs array
                IPs = Dns.GetHostEntry(host).AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray();
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): IPs.Length: {IPs.Length}");
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): se.NativeErrorCode: {se.NativeErrorCode}");
                return IPs[0];
            }

            foreach (IPAddress ipAddr in IPs)
            {
                byte[] myAddressBytes = ipAddr.GetAddressBytes();
                switch (myAddressBytes[0])
                {
                    case 10:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): myByte: {10}");
                        break;
                    case 172:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): myByte: {172}");
                        break;
                    case 192:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): myByte: {192}");
                        break;
                    default:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): myByte: {myAddressBytes[0]}");
                        break;
                }

                foreach (byte myByte in myAddressBytes)
                {
                    //_log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): myByte: {myByte}");
                }
                //_log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): ipAddr: {ipAddr}");
            }

            IPAddress ip = null;
            bool rs = IPAddress.TryParse(host, out IPAddress ipAddress);
            if (!rs)
            {
                foreach (IPAddress ipAddr in IPs)
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
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ResolveIp(): ipAddress: {ipAddress}");
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
            //_log.Log(consoleColor, $"TRSocketIPsViewModel::ParseResponseData()");
            //_log.Log( $"TRSocketIPsViewModel::ParseResponseData(): data: {data}");
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
            _log.Log(consoleColor, "TRSocketIPsViewModel::ParseOutputData()");

            string parsedData;
            parsedData = String.Format("{0}\r\n", outData);
            return parsedData;
        }
        #endregion ParseOutputData


        #region ConnectCallback
        private void ConnectCallback(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            //_log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectCallback(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            // Retrieve the socket from the state object.  
            Socket terminalSocket = (Socket)ar.AsyncState;
            try
            {
                // Complete the connection. Ends a pending asynchronous connection request.
                terminalSocket.EndConnect(ar); // throws an exception if connecting fails
                                               // Exception thrown: 'System.Net.Internals.SocketExceptionFactory.ExtendedSocketException' in System.Private.CoreLib.dll

                //_connectionItem.ConnectTime = DateTime.Now;
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectCallback(): socket: { terminalSocket.Handle} " +
                    $"connected to {terminalSocket.LocalEndPoint}");
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectCallback(): {se.NativeErrorCode} : {se.SocketErrorCode} : {se.Message}");
                if (!_mySocketErrorDict.ContainsKey(terminalSocket.Handle))
                {
                    _mySocketErrorDict.Add(terminalSocket.Handle, se);      // on multiple try we may get an error: 'An item with the same key has already been added'
                }
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectCallback(): {ex.Message}");
            }
            // Signal that the connection has been made.  (or not)
            connectDone.Set();
            //_log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectCallback(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method.");
        }
        #endregion ConnectCallback



        #region MyConnectAsync
        private async Task MyConnectAsync(EndPoint remoteEP, Socket terminalSocket)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::MyConnectAsync(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            IAsyncResult result = null;
            try
            {
                result = terminalSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), terminalSocket);
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::MyConnectAsync(): {ex.Message}");
            }

            // waiting for complition
            while (!result.IsCompleted)
            {
                await Task.Yield();
            }

            _log.Log(consoleColor, $"TRSocketIPsViewModel::MyConnectAsync(): terminalSocket.Handle: {terminalSocket.Handle}, result.IsCompleted: {result.IsCompleted}, _mySocketNativeErrorCode: {_mySocketNativeErrorCode}");

            _log.Log(consoleColor, $"TRSocketIPsViewModel::MyConnectAsync(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method.");
            //return (result is not null) ? result.IsCompleted : false;
        }
        #endregion MyConnectAsync



        #region ConnectToHost
        // method returns an error code

        
        //public SocketException ConnectToHost(IPAddress _ipAddress, Socket terminalSocket)
        public SocketException ConnectToHost(Tuple<Socket, IPAddress, int> myTuple)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectToHost(): socket: {myTuple.Item1.Handle}, " +
                $"ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            Task MyTask = Task.Run(async () =>
            {
                int connectTry = 2;    // how many try to connect?
                EndPoint remoteEP = new IPEndPoint(myTuple.Item2, myTuple.Item3);
                do
                {
                    await this.MyConnectAsync(remoteEP, myTuple.Item1);
                    connectDone.WaitOne(1000, true);

                    if (myTuple.Item1.Connected == false)
                    {
                        await Task.Delay(5000);
                        connectTry--;
                    }
                } while ((myTuple.Item1.Connected == false) && (connectTry > 0));

                _log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectToHost(): {myTuple.Item1.RemoteEndPoint} --> {myTuple.Item1.LocalEndPoint}");
            });
            MyTask.Wait();

            SocketException se = this.GetLastError(myTuple.Item1);
            return se;
        }
        #endregion ConnectToHost



        //#region ConnectToHost
        //// method returns an error code
        //public SocketException ConnectToHost(string myHost)
        //{
        //    _log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectToHost(): myHost: {myHost}, " +
        //        $"ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

        //    Task MyTask = Task.Run(async () =>
        //    {
        //        int connectTry = 1;    // how many try to connect?
        //        _ipAddress = ResolveIp(myHost);
        //        EndPoint remoteEP = new IPEndPoint(_ipAddress, _connectionItem.Port);
        //        do
        //        {
        //            await this.MyConnectAsync(remoteEP);
        //            connectDone.WaitOne();

        //            if (IsConnected() == false)
        //            {
        //                await Task.Delay(5000);
        //                connectTry--;
        //            }
        //        } while ((IsConnected() == false) && (connectTry > 0));

        //        _log.Log(consoleColor, $"TRSocketIPsViewModel::ConnectToHost(): {terminalSocket.RemoteEndPoint} --> {terminalSocket.LocalEndPoint}");
        //    });
        //    MyTask.Wait();

        //    SocketException se = this.GetLastError();
        //    return se;
        //}
        //#endregion ConnectToHost


        // To cancel a pending BeginReceive one can call the Close method.
        #region ReceiveCallBack
        private void ReceiveCallBack(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            // Retrieve the state object and the client socket from the asynchronous state object.  
            StateObject so = (StateObject)ar.AsyncState;
            Socket terminalSocket = so.workSocket;

            string response = "", tmpResponse = "";
            string heartbeat = "@\r\n";
            try
            {
                // the socket server returns 0x0d for a new line, and 0x0d 0x0a for end of message
                int receivedBytes = terminalSocket.EndReceive(ar); //   Ends a pending asynchronous read.
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}, receivedBytes: {receivedBytes}");

                // Connection error occured, as the server never returns 0 bytes
                if (receivedBytes == 0)
                {
                    _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}: It looks like a connection error!");
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
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveCallBack(): {se.NativeErrorCode} : {se.SocketErrorCode} : {se.Message}");
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveCallBack(): {ex.Message}");
            }
            receiveDone.Set();
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion ReceiveCallBack



        #region ReceiveFromSocket
        public string ReceiveFromSocket(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveFromSocket(): " +
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

            _log.Log(consoleColor, $"TRSocketIPsViewModel::ReceiveFromSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method");
            return state.sb.ToString();
        }
        #endregion ReceiveFromSocket



        #region SendCallBack
        private void SendCallBack(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            _log.Log(consoleColor, $"TRSocketIPsViewModel::SendCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            // Retrieve the socket from the state object.  
            Socket terminalSocket = (Socket)ar.AsyncState;
            _log.Log(consoleColor, $"TRSocketIPsViewModel::SendCallBack: socket: {terminalSocket.Handle}");

            try
            {
                // Complete sending the data to the remote device.  
                int bytesSend = terminalSocket.EndSend(ar);
                _log.Log(consoleColor, $"TRSocketIPsViewModel::SendCallBack: bytesSend: {bytesSend}");

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::SendCallBack: {ex.Message}");
            }
            _log.Log(consoleColor, $"TRSocketIPsViewModel::SendCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion SendCallBack


        #region SendToSocket
        // BeginSend() Sends data asynchronously to a connected System.Net.Sockets.Socket.
        public string SendToSocket(Socket terminalSocket, string text)
        {
            if (terminalSocket == null) return String.Empty;
            if (IsConnected(terminalSocket) is false) return String.Empty;

            _log.Log(consoleColor, $"TRSocketIPsViewModel::SendToSocket(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

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
                _log.Log(consoleColor, $"TRSocketIPsViewModel::SendToSocket(): {se.NativeErrorCode} : {se.SocketErrorCode}: {se.Message}");
            }
            sendDone.WaitOne();

            string response = this.ReceiveFromSocket(terminalSocket);

            _log.Log(consoleColor, $"TRSocketIPsViewModel::SendToSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
            return response;
        }
        #endregion SendToSocket





        #region DisconnectCallback
        private void DisconnectCallback(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            _log.Log(consoleColor, "TRSocketIPsViewModel::DisconnectCallback(): Start of method");

            // Complete the disconnect request.
            Socket client = (Socket)ar.AsyncState;
            _log.Log(consoleColor, $"TRSocketIPsViewModel::DisconnectCallback(): socket: {client.Handle}");

            try
            {
                client.EndDisconnect(ar);  // Ends a pending asynchronous disconnect request.
                                           //client.Close();     // nie za bardzo..
                _log.Log(consoleColor, $"TRSocketIPsViewModel::DisconnectCallback: The terminal is disconnected!");
            }
            catch (SocketException se)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::DisconnectCallback: {se.ErrorCode}: {se.Message}");
            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::DisconnectCallback: {ex.Message}");
            }
            // Signal that the disconnect is complete.
            disconnectDone.Set();
            _log.Log(consoleColor, "TRSocketIPsViewModel::DisconnectCallback(): End of method");
        }
        #endregion

        #region Disconnect
        // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.begindisconnect?view=net-6.0
        public bool Disconnect(Socket terminalSocket)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::Disconnect(): socket: {terminalSocket.Handle} - Start of method");
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
                        _log.Log(consoleColor, "TRSocketIPsViewModel::Disconnect(): We're still connected");
                        result = false;
                    }
                    else
                    {
                        _log.Log(consoleColor, "TRSocketIPsViewModel::Disconnect(): We're disconnected");
                        result = true;
                    }
                }
                catch (SocketException se)
                {
                    _log.Log(consoleColor, $"TRSocketIPsViewModel::Disconnect(): {se.SocketErrorCode} : {se.Message}");
                    result = false;
                }
                catch (Exception ex)
                {
                    _log.Log(consoleColor, $"TRSocketIPsViewModel::Disconnect(): {ex.Message}");
                    result = false;
                }

            }
            _log.Log(consoleColor, $"TRSocketIPsViewModel::Disconnect(): End of method: result: {result}");
            return result;
        }
        #endregion



        ////////////////// CLOSING SOCKETS /////////////////
        private void RunCloseCommandMessage()
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::RunCloseCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            List<Socket> myInitializedListOfSockets = new List<Socket>();
            Parallel.ForEach(_myListOfSockets, (mySocket) =>
            {
                this.Close(mySocket);
                _log.Log(consoleColor, $"TRSocketIPsViewModel::CloseAllSocketsParallel(): mySocket {mySocket.Handle}: {mySocket.Connected}");
            });

            _log.Log(consoleColor, $"TRSocketIPsViewModel::RunCloseCommandMessage(): End of method  ({this.GetHashCode():x8})");
        }

        private MyUser GetTRSocketUser()
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::GetTRSocketUser()");
            return new MyUser("MyTRSocketUser");
        }

        #region RunTRShutdownCommand
        private TRSocketStateMessage RunTRShutdownCommand()
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::RunTRShutdownCommand(): Start of method  ({this.GetHashCode():x8})");

            if (_myListOfSockets == null)
                return new TRSocketStateMessage { TRErrorNumber = -1 };

            bool shutDownResult = true;
            int numberOfDisconnectedSockets = 0;


            //Parallel.ForEach(_myListOfSockets, (mySocket) => { });

            // obs: parallel call
            Parallel.ForEach(_myListOfSockets, (mySocket) =>
            {
                bool rs = false;
                rs = this.Disconnect(mySocket);
                _log.Log(consoleColor, $"TRSocketIPsViewModel::RunTRShutdownCommand(): mySocket {mySocket.Handle}: {mySocket.Connected}");
                if (rs)
                {
                    numberOfDisconnectedSockets++;
                }
            });

            _log.Log(consoleColor, $"TRSocketIPsViewModel::RunTRShutdownCommand(): numberOfDisconnectedSockets: {numberOfDisconnectedSockets}, _myListOfSockets.Count: {_myListOfSockets.Count}");
            if (numberOfDisconnectedSockets != _myListOfSockets.Count)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::RunTRShutdownCommand(): not all sockets have been shuted down properly.");
                shutDownResult = false;
            }

            TRSocketStateMessage trssm = new TRSocketStateMessage();
            trssm.TRErrorNumber = shutDownResult ? 0 : -1;      // error number can expand
            trssm.MyStateName = "TRSocketIPsViewModel";
            trssm.trStatus = shutDownResult ? TRStatus.Success : TRStatus.Error;

            _log.Log(consoleColor, $"TRSocketIPsViewModel::RunTRShutdownCommand(): End of method  ({this.GetHashCode():x8})");
            return trssm;
        }
        #endregion RunTRShutdownCommand


        #region RunTRInitCommandMessage
        ///////////// INITIALIZING SOCKETS /////////////////
        // async doesn't work well with Parallel.ForEach: https://stackoverflow.com/a/23139769/7036047
        // The whole idea behind Parallel.ForEach() is that you have a set of threads and each thread processes part of the collection.
        // This doesn't work with async-await, where you want to release the thread for the duration of the async call.
        private TRSocketStateMessage TRSocketInitAsync()
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketInitAsync(): Start of method  ({this.GetHashCode():x8})");

            _connectionItemList = new List<Tuple<Socket, IPAddress, int>>();
            TRSocketStateMessage trssm = new TRSocketStateMessage { MyStateName = "TRSocketInitAsync" };
            
            bool rs;
            Socket mySocket;
            LingerOption lingerOption = new LingerOption(true, 5);
            foreach (ConnectionItem ci in _connectionItems)
            {
                rs = IPAddress.TryParse(ci.Host, out IPAddress ipAddress);
                _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketInitAsync(): rs: {rs}");
                if (!rs)
                    return trssm;

                mySocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 500,
                    ReceiveTimeout = 1000
                };
                mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

                _connectionItemList.Add(new Tuple<Socket, IPAddress, int>(mySocket, ipAddress, ci.Port));
            }
            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketInitAsync(): _connectionItemList.Count: {_connectionItemList.Count}");


            _myListOfSockets = new List<Socket>();
            Dictionary<IntPtr, Tuple<int, string>> MyInitSocketDict = new();
            SocketException se = null;
            foreach (Tuple<Socket, IPAddress, int> myConnectionItem in _connectionItemList)
            {
                if (!myConnectionItem.Item1.Connected)
                {
                    se = this.ConnectToHost(myConnectionItem);
                }
                if (se != null)
                {
                    Tuple<int, string> myTuple = new Tuple<int, string>(se.NativeErrorCode, se.Message);
                    MyInitSocketDict.Add(myConnectionItem.Item1.Handle, myTuple);
                    trssm.SocketInitDict = MyInitSocketDict;
                }
                else
                {
                    ///// receiving the hello message /////
                    string response = this.ReceiveFromSocket(myConnectionItem.Item1);
                    _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketInitAsync(): Socket {myConnectionItem.Item1.Handle} with address {myConnectionItem.Item1.RemoteEndPoint} got response: {response}");
                    _myListOfSockets.Add(myConnectionItem.Item1);
                    MyInitSocketDict.Add(myConnectionItem.Item1.Handle, new Tuple<int, string>(0, response));   // error_code is 0 in return
                    trssm.SocketInitDict = MyInitSocketDict;
                }
            }
            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketInitAsync(): _myListOfSockets.Count: {_myListOfSockets.Count}");

            // we are ready with initialize
            _log.Log(consoleColor, $"TRSocketIPsViewModel::RunTRInitCommandMessage(): End of method  ({this.GetHashCode():x8})");
            return trssm;
        }
        #endregion RunTRInitCommandMessage


        //#region TRSocketInitAsync
        ////this method should not return any value until all sockets have been initialized
        //private async Task<TRSocketStateMessage> TRSocketInitAsync()
        //{
        //    TRSocketStateMessage trssm = new TRSocketStateMessage();
        //    bool rs = await Task.Run(RunTRInitCommandMessage);
        //    trssm.TRErrorNumber = rs ? 0 : -1;      // error number can expand
        //    trssm.MyStateName = "TRSocketInitAsync";
        //    trssm.trStatus = rs ? TRStatus.Success : TRStatus.Error;

        //    //return new TRSocketStateMessage { MyStateName = "TRSocketIPsViewModel", trStatus = rs ? TRStatus.Success : TRStatus.Error };
        //    return trssm;
        //}
        //#endregion TRSocketInitAsync





        #region TRSocketCheckPowerSupplyCommand
        private string ParseResponseString(string response, string myParameterIamLookingFor)
        {
            //_log.Log(consoleColor, $"TRSocketIPsViewModel::ParseResponseString(): Start of method ");

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

            //_log.Log(consoleColor, $"TRSocketIPsViewModel::ParseResponseString(): End of method ");
            return powerInputResult;
        }

        // '30' command
        private TRSocketStateMessage TRSocketCheckPowerSupplyCommand()
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketCheckPowerSupplyCommand(): Start of method  ({this.GetHashCode():x8})");

            double myVoltage = 0.0;
            //CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            CultureInfo cultureInfo = new("en-US");
            NumberStyles styles = NumberStyles.Number;

            int TRErrorCode = 0;
            TRSocketStateMessage trssm = new();
            trssm.MyStateName = "TRSocketCheckPowerSupplyCommand";
            Dictionary<IntPtr, Tuple<string, double, int>> MySocketDict = new();

            if (_myListOfSockets.Count == 0)
            {
                MySocketDict.Add((IntPtr)0, new Tuple<string, double, int>("", 0.0, -1));
            }

            foreach (Socket terminalSocket in _myListOfSockets)
            {
                string myResponse30 = this.SendToSocket(terminalSocket, ParseOutputData("30"));
                string myResponsePowerInput = this.ParseResponseString(myResponse30, "Power Input");
                string myResponseACIn = this.ParseResponseString(myResponse30, "AC In");
                string[] responseLines = myResponseACIn.Split(' ', StringSplitOptions.None);

                foreach (string myVoltageLine in responseLines)
                {
                    if (!myVoltageLine.Contains('V'))
                        continue;

                    bool isDouble = double.TryParse(myVoltageLine.Trim('V'), styles, cultureInfo, out myVoltage);
                    if (isDouble)
                        TRErrorCode = (myVoltage < 240 && myVoltage > 200) ? 0 : -2;
                    else
                        TRErrorCode = -3;   // Could'nt parse the voltage value
                }

                Tuple<string, double, int> MyTuple = new(myResponsePowerInput, myVoltage, TRErrorCode);
                MySocketDict.Add(terminalSocket.Handle, MyTuple);
            }

            trssm.CheckPowerSupplyDict = MySocketDict;

            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketCheckPowerSupplyCommand(): End of method  ({this.GetHashCode():x8})");
            return trssm;
        }
        #endregion TRSocketCheckPowerSupplyCommand



        // Display Battery Mode. Ref.: SLUUBD3D 17.5 0x03 BatteryMode()
        // CAPM(Bit 15) : CAPACITY Mode
        // CHGM (Bit 14) : CHARGER Mode
        // AM (Bit 13) : ALARM Mode
        // Bit 12..10 : reserved
        // PB(Bit 9) : Primary Battery
        // CC(Bit 8) : Charge Controller Enabled
        // CF(Bit 7) : Condition Flag (R). This bit a the same as GaugingStatus() [CF]
        // Bit 6..2 : reserved
        // PBS(Bit 1) : Primary Battery Support
        // ICC(Bit 0) : Internal Charge Controller
        private UInt16 ParseBatteryMode(string myBatteryMode)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryMode(): myBatteryStatus: {myBatteryMode}");

            UInt16 myInt16Value = Convert.ToUInt16(myBatteryMode, 16);
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryMode(): myInt16Value: {myInt16Value}");

            bool CAPACITYMode = Convert.ToBoolean(myInt16Value & 0x8000) ? true : false;
            if (CAPACITYMode)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Reports specific data in 10 mW or 10 mWh");
            else
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Reports specific data in mA or mAh(default)");

            bool CHARGERMode = Convert.ToBoolean(myInt16Value & 0x4000) ? true : false;
            if (CHARGERMode)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Disable ChargingVoltage() and ChargingCurrent() broadcasts to host and smart battery charger");
            else
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Enable ChargingVoltage() and ChargingCurrent() broadcasts to host and smart battery charger(default)");

            bool ALARMMode = Convert.ToBoolean(myInt16Value & 0x2000) ? true : false;
            if (ALARMMode)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Disable Alarm Warning broadcasts to host and smart battery charger(default)");
            else
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Enable AlarmWarning broadcasts to host and smart battery charger");

            bool PrimaryBattery = Convert.ToBoolean(myInt16Value & 0x0200) ? true : false;
            if (PrimaryBattery)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Battery is operating in its secondary role.");
            else
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Battery is operating in its primary role(default).");

            bool ChargeControllerEnabled = Convert.ToBoolean(myInt16Value & 0x0100) ? true : false;
            if (ChargeControllerEnabled)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Internal charge control enabled");
            else _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Internal charge controller disabled(default)");

            bool ConditionFlag = Convert.ToBoolean(myInt16Value & 0x0080) ? true : false;     // GaugingStatus
            if (ConditionFlag)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Conditioning cycle requested");
            else
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Battery OK");

            bool PrimaryBatterySupport = Convert.ToBoolean(myInt16Value & 0x0002) ? true : false;
            if (PrimaryBatterySupport)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): PrimaryBatterySupport: Primary or Secondary Battery Support");
            else
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): PrimaryBatterySupport: Function not supported(default)");

            bool InternalChargeController = Convert.ToBoolean(myInt16Value & 0x0001) ? true : false;
            if (InternalChargeController)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): InternalChargeController: Function supported(default)");
            else
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): InternalChargeController: Function not supported");


            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryMode(): ParseBatteryStatus: End of method");
            return myInt16Value;
        }



        // Display Battery Status. Ref.: SLUUBD3D 17.24 0x16 BatteryStatus()
        //      B15: OCA, Overcharged Alarm (Ref.: SLUUBD3D part 3.10)
        //	    B14: TCA, Terminate Charge Alarm (Ref.: SLUUBD3D part 3.3)
        //      B13: RSVD(Bit 13) : Reserved
        //	    B12: OTA, Overtemperature Alarm (Ref.: SLUUBD3D part 3.7)
        //	    B11: TDA, Terminate Discharge Alarm (Ref.: SLUUBD3D part 3.2)
        //      B10: RSVD(Bit 10) : Reserved
        //	    B9: RCA, Remaining Capacity Alarm (Ref.: SLUUBD3D part 17.3)
        //	    B8: RTA, Remaining Time Alarm (Ref.: SLUUBD3D part 17.4)
        //      INIT(Bit 7) : Initialization
        //      DSG(Bit 6): Discharging or Rest
        //      FC(Bit 5) : Fully Charged
        //      FD (Bit 4): Fully Discharged
        //      EC3:0 (Bits 3–0): Error Code
        private UInt16 ParseBatteryStatus(string myBatteryStatus)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): myBatteryStatus: {myBatteryStatus}");

            UInt16 myInt16Value = Convert.ToUInt16(myBatteryStatus, 16);
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): myInt16Value: {myInt16Value}");

            bool OverchargedAlarm = Convert.ToBoolean(myInt16Value & 0x8000) ? true : false;
            if (OverchargedAlarm)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): OverchargedAlarm: {OverchargedAlarm}");

            bool TerminateChargeAlarm = Convert.ToBoolean(myInt16Value & 0x4000) ? true : false;
            if (TerminateChargeAlarm)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): TerminateChargeAlarm: {TerminateChargeAlarm}");

            bool OvertemperatureAlarm = Convert.ToBoolean(myInt16Value & 0x1000) ? true : false;
            if (OvertemperatureAlarm)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): OvertemperatureAlarm: {OvertemperatureAlarm}");

            bool TerminateDischargeAlarm = Convert.ToBoolean(myInt16Value & 0x0800) ? true : false;
            if (TerminateDischargeAlarm)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): TerminateDischargeAlarm: {TerminateDischargeAlarm}");

            bool RemainingCapacityAlarm = Convert.ToBoolean(myInt16Value & 0x0200) ? true : false;
            if (RemainingCapacityAlarm)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): RemainingCapacityAlarm: {RemainingCapacityAlarm}");

            bool RemainingTimeAlarm = Convert.ToBoolean(myInt16Value & 0x0100) ? true : false;
            if (RemainingTimeAlarm)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): RemainingTimeAlarm: {RemainingTimeAlarm}");

            bool Initialization = Convert.ToBoolean(myInt16Value & 0x0080) ? true : false;
            if (Initialization)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): Initialization: {Initialization}");

            bool DischargingOrRest = Convert.ToBoolean(myInt16Value & 0x0040) ? true : false;
            if (DischargingOrRest)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): DischargingOrRest: {DischargingOrRest}");

            bool FullyCharged = Convert.ToBoolean(myInt16Value & 0x0020) ? true : false;
            if (FullyCharged)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): FullyCharged: {FullyCharged}");

            bool FullyDischarged = Convert.ToBoolean(myInt16Value & 0x0010) ? true : false;
            if (FullyDischarged)
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): FullyDischarged: {FullyDischarged}");

            bool ErrorCode = Convert.ToBoolean(myInt16Value & 0x000F) ? true : false;
            if (ErrorCode)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): ErrorCode: {ErrorCode}");
                switch (myInt16Value & 0x000F)
                {
                    case 0:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: OK");
                        break;
                    case 1:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: Busy");
                        break;
                    case 2:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: Reserved Command");
                        break;
                    case 3:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: Unsupported Command");
                        break;
                    case 4:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: AccessDenied");
                        break;
                    case 5:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: Overflow/Underflow");
                        break;
                    case 6:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: BadSize");
                        break;
                    case 7:
                        _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): case: UnknownError");
                        break;
                    default:
                        break;
                }
            }

            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseBatteryStatus(): ParseBatteryStatus: End of method");
            if ((!ErrorCode) && (!OverchargedAlarm) && (!OvertemperatureAlarm) && (!RemainingCapacityAlarm) && (!RemainingTimeAlarm)) return 0;
            else return myInt16Value;
        }


        //0x0050 SafetyAlert (32bits)
        //OC(Bit 20) : Overcharge (0x00100000)
        //CTOS(Bit 19) : Charge Timeout Suspend (0x00080000)
        //RSVD(Bit 18) : Reserved
        //PTOS(Bit 17): Precharge Timeout Suspend (0x00020000)
        //RSVD(Bits 16–15) : Reserved
        //OCDL(Bit 14): Overcurrent During Discharge Latch (0x00004000)
        //OTF(Bit 13) : Overtemperature Fault (0x00002000)
        //AFE_OVRD(Bit 12) : AFE Alert (0x00001000)
        //UTD(Bit 11) : Undertemperature During Discharge (0x00000800)
        //UTC(Bit 10) : Undertemperature During Charge (0x00000400)
        //OTD(Bit 9) : Overtemperature During Discharge (0x00000200)
        //OTC(Bit 8) : Overtemperature During Charge (0x00000100)
        //ASCDL(Bit 7) : Short Circuit During Discharge Latch (0x00000080)
        //ASCD(Bit 6) : Short Circuit During Discharge (0x00000040)
        //AOLDL(Bit 5) : Overload During Discharge Latch (0x00000020)
        //AOLD(Bit 4) : Overload During Discharge (0x00000010)
        //OCD(Bit 3) : Overcurrent During Discharge (0x00000008)
        //OCC(Bit 2) : Overcurrent During Charge (0x00000004)
        //COV(Bit 1) : Cell Overvoltage (0x00000002)
        //CUV(Bit 0) : Cell Undervoltage (0x00000001)
        private UInt32 ParseSafetyAlert(string mySafetyAlertStr)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): Start of method");

            bool result = false;

            UInt32 myInt32Value = Convert.ToUInt32(mySafetyAlertStr, 16);
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): myInt32Value: {myInt32Value}");

            bool Overcharge = Convert.ToBoolean(myInt32Value & 0x00100000) ? true : false;
            if (Overcharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): Overcharge: {Overcharge}");
                //result = SafetyAlertEnum.Overcharge | SafetyAlertEnum.;
                result = false;
            }

            bool ChargeTimeoutSuspend = Convert.ToBoolean(myInt32Value & 0x00080000) ? true : false;
            if (ChargeTimeoutSuspend)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ChargeTimeoutSuspend: {ChargeTimeoutSuspend}");
                result = false;
            }

            bool PrechargeTimeoutSuspend = Convert.ToBoolean(myInt32Value & 0x00020000) ? true : false;
            if (PrechargeTimeoutSuspend)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): PrechargeTimeoutSuspend: {PrechargeTimeoutSuspend}");
                result = false;
            }

            bool OvercurrentDuringDischargeLatch = Convert.ToBoolean(myInt32Value & 0x00004000) ? true : false;
            if (OvercurrentDuringDischargeLatch)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvercurrentDuringDischargeLatch: {OvercurrentDuringDischargeLatch}");
                result = false;
            }

            bool OvertemperatureFault = Convert.ToBoolean(myInt32Value & 0x00002000) ? true : false;
            if (OvertemperatureFault)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvertemperatureFault: {OvertemperatureFault}");
                result = false;
            }

            bool AFEAlert = Convert.ToBoolean(myInt32Value & 0x00001000) ? true : false;
            if (AFEAlert)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): AFEAlert: {AFEAlert}");
                result = false;
            }

            bool UndertemperatureDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000800) ? true : false;
            if (UndertemperatureDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): UndertemperatureDuringDischarge: {UndertemperatureDuringDischarge}");
                result = false;
            }

            bool UndertemperatureDuringCharge = Convert.ToBoolean(myInt32Value & 0x00000400) ? true : false;
            if (UndertemperatureDuringCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): UndertemperatureDuringCharge: {UndertemperatureDuringCharge}");
                result = false;
            }

            bool OvertemperatureDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000200) ? true : false;
            if (OvertemperatureDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvertemperatureDuringDischarge: {OvertemperatureDuringDischarge}");
                result = false;
            }

            bool OvertemperatureDuringCharge = Convert.ToBoolean(myInt32Value & 0x00000100) ? true : false;
            if (OvertemperatureDuringCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvertemperatureDuringCharge: {OvertemperatureDuringCharge}");
                result = false;
            }

            bool ShortCircuitDuringDischargeLatch = Convert.ToBoolean(myInt32Value & 0x00000080) ? true : false;
            if (ShortCircuitDuringDischargeLatch)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ShortCircuitDuringDischargeLatch: {ShortCircuitDuringDischargeLatch}");
                result = false;
            }

            bool ShortCircuitDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000040) ? true : false;
            if (ShortCircuitDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ShortCircuitDuringDischarge: {ShortCircuitDuringDischarge}");
                result = false;
            }

            bool OverloadDuringDischargeLatch = Convert.ToBoolean(myInt32Value & 0x00000020) ? true : false;
            if (OverloadDuringDischargeLatch)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OverloadDuringDischargeLatch: {OverloadDuringDischargeLatch}");
                result = false;
            }

            bool OverloadDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000010) ? true : false;
            if (OverloadDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OverloadDuringDischarge: {OverloadDuringDischarge}");
                result = false;
            }

            bool OvercurrentDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000008) ? true : false;
            if (OvercurrentDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvercurrentDuringDischarge : {OvercurrentDuringDischarge }");
                result = false;
            }

            bool OvercurrentDuringCharge = Convert.ToBoolean(myInt32Value & 0x00000004) ? true : false;
            if (OvercurrentDuringCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvercurrentDuringCharge : {OvercurrentDuringCharge }");
                result = false;
            }

            bool CellOvervoltage = Convert.ToBoolean(myInt32Value & 0x00000002) ? true : false;
            if (CellOvervoltage)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CellOvervoltage : {CellOvervoltage }");
                result = false;
            }

            bool CellUndervoltage = Convert.ToBoolean(myInt32Value & 0x00000001) ? true : false;
            if (CellUndervoltage)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CellUndervoltage : {CellUndervoltage }");
                result = false;
            }

            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): End of method");
            //return result;
            return myInt32Value;
        }



        //0x0051 SafetyStatus (32bits)
        //RSVD(Bits 31–21) : Reserved 
        //OC(Bit 20): Overcharge (0x00100000)
        //RSVD(Bit 19) : Reserved (0x00080000)
        //CTO(Bit 18): Charge Timeout (0x00040000)
        //RSVD(Bit 17) : Reserved (0x00020000)
        //PTO(Bit 16): Precharge Timeout (0x00010000)
        //RSVD(Bits 15) : Reserved (0x00008000)
        //OCDL(Bit 14): Overcurrent During Discharge Latch (0x00004000)
        //OTF(Bit 13) : Overtemperature Fault (0x00002000)
        //AFE_OVRD(Bit 12) : AFE Alert (0x00001000)
        //UTD(Bit 11) : Undertemperature During Discharge (0x00000800)
        //UTC(Bit 10) : Undertemperature During Charge (0x00000400)
        //OTD(Bit 9) : Overtemperature During Discharge (0x00000200)
        //OTC(Bit 8) : Overtemperature During Charge (0x00000100)
        //ASCDL(Bit 7) : Short Circuit During Discharge Latch (0x00000080)
        //ASCD(Bit 6) : Short Circuit During Discharge (0x00000040)
        //AOLDL(Bit 5) : Overload During Discharge Latch (0x00000020)
        //AOLD(Bit 4) : Overload During Discharge (0x00000010)
        //OCD(Bit 3) : Overcurrent During Discharge (0x00000008)
        //OCC(Bit 2) : Overcurrent During Charge (0x00000004)
        //COV(Bit 1) : Cell Overvoltage (0x00000002)
        //CUV(Bit 0) : Cell Undervoltage (0x00000001)
        private UInt32 ParseSafetyStatus(string mySafetyStatusStr)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyStatus(): Start of method");

            bool result = false;
            UInt32 myInt32Value = Convert.ToUInt32(mySafetyStatusStr, 16);
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyStatus(): myInt32Value: {myInt32Value}");

            bool Overcharge = Convert.ToBoolean(myInt32Value & 0x00100000) ? true : false;
            if (Overcharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): Overcharge: {Overcharge}");
                result = false;
            }

            bool ChargeTimeout = Convert.ToBoolean(myInt32Value & 0x00040000) ? true : false;
            if (ChargeTimeout)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ChargeTimeout: {ChargeTimeout}");
                result = false;
            }

            bool PrechargeTimeout = Convert.ToBoolean(myInt32Value & 0x00010000) ? true : false;
            if (PrechargeTimeout)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): PrechargeTimeout: {PrechargeTimeout}");
                result = false;
            }

            bool OvercurrentDuringDischargeLatch = Convert.ToBoolean(myInt32Value & 0x00004000) ? true : false;
            if (OvercurrentDuringDischargeLatch)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvercurrentDuringDischargeLatch: {OvercurrentDuringDischargeLatch}");
                result = false;
            }

            bool OvertemperatureFault = Convert.ToBoolean(myInt32Value & 0x00002000) ? true : false;
            if (OvertemperatureFault)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvertemperatureFault: {OvertemperatureFault}");
                result = false;
            }

            bool AFEAlert = Convert.ToBoolean(myInt32Value & 0x00001000) ? true : false;
            if (AFEAlert)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): AFEAlert: {AFEAlert}");
                result = false;
            }

            bool UndertemperatureDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000800) ? true : false;
            if (UndertemperatureDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): UndertemperatureDuringDischarge: {UndertemperatureDuringDischarge}");
                result = false;
            }

            bool UndertemperatureDuringCharge = Convert.ToBoolean(myInt32Value & 0x00000400) ? true : false;
            if (UndertemperatureDuringCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): UndertemperatureDuringCharge: {UndertemperatureDuringCharge}");
                result = false;
            }

            bool OvertemperatureDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000200) ? true : false;
            if (OvertemperatureDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvertemperatureDuringDischarge: {OvertemperatureDuringDischarge}");
                result = false;
            }

            bool OvertemperatureDuringCharge = Convert.ToBoolean(myInt32Value & 0x00000100) ? true : false;
            if (OvertemperatureDuringCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvertemperatureDuringCharge: {OvertemperatureDuringCharge}");
                result = false;
            }

            bool ShortCircuitDuringDischargeLatch = Convert.ToBoolean(myInt32Value & 0x00000080) ? true : false;
            if (ShortCircuitDuringDischargeLatch)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ShortCircuitDuringDischargeLatch: {ShortCircuitDuringDischargeLatch}");
                result = false;
            }

            bool ShortCircuitDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000040) ? true : false;
            if (ShortCircuitDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ShortCircuitDuringDischarge: {ShortCircuitDuringDischarge}");
                result = false;
            }

            bool OverloadDuringDischargeLatch = Convert.ToBoolean(myInt32Value & 0x00000020) ? true : false;
            if (OverloadDuringDischargeLatch)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OverloadDuringDischargeLatch: {OverloadDuringDischargeLatch}");
                result = false;
            }

            bool OverloadDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000010) ? true : false;
            if (OverloadDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OverloadDuringDischarge: {OverloadDuringDischarge}");
                result = false;
            }

            bool OvercurrentDuringDischarge = Convert.ToBoolean(myInt32Value & 0x00000008) ? true : false;
            if (OvercurrentDuringDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvercurrentDuringDischarge: {OvercurrentDuringDischarge}");
                result = false;
            }

            bool OvercurrentDuringCharge = Convert.ToBoolean(myInt32Value & 0x00000004) ? true : false;
            if (OvercurrentDuringCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OvercurrentDuringCharge: {OvercurrentDuringCharge}");
                result = false;
            }

            bool CellOvervoltage = Convert.ToBoolean(myInt32Value & 0x00000002) ? true : false;
            if (CellOvervoltage)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CellOvervoltage: {CellOvervoltage}");
                result = false;
            }

            bool CellUndervoltage = Convert.ToBoolean(myInt32Value & 0x00000001) ? true : false;
            if (CellUndervoltage)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CellUndervoltage: {CellUndervoltage}");
                result = false;
            }

            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyStatus(): End of method");
            //return result;
            return myInt32Value;
        }





        //0x0052 PFAlert (16bits)
        //SOTF(Bit 15) : SafetyOvertemperatureFETFailure (0x8000)
        //TS3(Bit 14) : OpenThermistorTS3Failure (0x4000)
        //TS2(Bit 13) : OpenThermistorTS2Failure (0x2000)
        //TS1(Bit 12) : OpenThermistorTS1Failure (0x1000)
        //AFE_XRDY(Bit 11) : CompanionBQ769x0AFEXREADYFailure (0x0800)
        //AFE_OVRD(Bit 10) : CompanionBQ769x0AFEOverrideFailure (0x0400)
        //AFEC(Bit 9) : AFE CommunicationFailure (0x0200)
        //AFER(Bit 8) : AFE RegisterFailure (0x0100)
        //DFETF(Bit 7) : DischargeFETFailure (0x0080)
        //CFETF(Bit 6) : ChargeFETFailure (0x0040)
        //VIMR(Bit 5) : VoltageImbalanceWhilePackIsAtRestFailure (0x0020)
        //SOT(Bit 4) : SafetyOvertemperatureCellFailure (0x0010)
        //SOCD(Bit 3) : SafetyOvercurrentInDischarge (0x0008)
        //SOCC(Bit 2) : SafetyOvercurrentInCharge (0x0004)
        //SOV(Bit 1) : SafetyCellOvervoltageFailure (0x0002)
        //SUV(Bit 0) : SafetyCellUndervoltageFailure (0x0001)
        private ushort ParsePFAlert(string myPFAlertStr)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParsePFAlert(): Start of method");
            bool result = false;

            ushort myInt16Value = Convert.ToUInt16(myPFAlertStr, 16);
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParsePFAlert(): myInt16Value: {myInt16Value}");

            bool SafetyOvertemperatureFETFailure = Convert.ToBoolean(myInt16Value & 0x8000) ? true : false;
            if (SafetyOvertemperatureFETFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvertemperatureFETFailure: {SafetyOvertemperatureFETFailure}");
                result = false;
            }

            bool OpenThermistorTS3Failure = Convert.ToBoolean(myInt16Value & 0x4000) ? true : false;
            if (OpenThermistorTS3Failure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OpenThermistorTS3Failure: {OpenThermistorTS3Failure}");
                result = false;
            }

            bool OpenThermistorTS2Failure = Convert.ToBoolean(myInt16Value & 0x2000) ? true : false;
            if (OpenThermistorTS2Failure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OpenThermistorTS2Failure: {OpenThermistorTS2Failure}");
                result = false;
            }

            bool OpenThermistorTS1Failure = Convert.ToBoolean(myInt16Value & 0x1000) ? true : false;
            if (OpenThermistorTS1Failure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OpenThermistorTS1Failure: {OpenThermistorTS1Failure}");
                result = false;
            }

            bool CompanionBQ769x0AFEXREADYFailure = Convert.ToBoolean(myInt16Value & 0x0800) ? true : false;
            if (CompanionBQ769x0AFEXREADYFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CompanionBQ769x0AFEXREADYFailure: {CompanionBQ769x0AFEXREADYFailure}");
                result = false;
            }

            bool CompanionBQ769x0AFEOverrideFailure = Convert.ToBoolean(myInt16Value & 0x0400) ? true : false;
            if (CompanionBQ769x0AFEOverrideFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CompanionBQ769x0AFEOverrideFailure: {CompanionBQ769x0AFEOverrideFailure}");
                result = false;
            }

            bool AFECommunicationFailure = Convert.ToBoolean(myInt16Value & 0x0200) ? true : false;
            if (AFECommunicationFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): AFECommunicationFailure: {AFECommunicationFailure}");
                result = false;
            }

            bool AFERegisterFailure = Convert.ToBoolean(myInt16Value & 0x0100) ? true : false;
            if (AFERegisterFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): AFERegisterFailure: {AFERegisterFailure}");
                result = false;
            }

            bool DischargeFETFailure = Convert.ToBoolean(myInt16Value & 0x0080) ? true : false;
            if (DischargeFETFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): DischargeFETFailure: {DischargeFETFailure}");
                result = false;
            }

            bool ChargeFETFailure = Convert.ToBoolean(myInt16Value & 0x0040) ? true : false;
            if (ChargeFETFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ChargeFETFailure: {ChargeFETFailure}");
                result = false;
            }

            bool VoltageImbalanceWhilePackIsAtRestFailure = Convert.ToBoolean(myInt16Value & 0x0020) ? true : false;
            if (VoltageImbalanceWhilePackIsAtRestFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): VoltageImbalanceWhilePackIsAtRestFailure: {VoltageImbalanceWhilePackIsAtRestFailure}");
                result = false;
            }

            bool SafetyOvertemperatureCellFailure = Convert.ToBoolean(myInt16Value & 0x0010) ? true : false;
            if (SafetyOvertemperatureCellFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvertemperatureCellFailure: {SafetyOvertemperatureCellFailure}");
                result = false;
            }

            bool SafetyOvercurrentInDischarge = Convert.ToBoolean(myInt16Value & 0x0008) ? true : false;
            if (SafetyOvercurrentInDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvercurrentInDischarge: {SafetyOvercurrentInDischarge}");
                result = false;
            }

            bool SafetyOvercurrentInCharge = Convert.ToBoolean(myInt16Value & 0x0004) ? true : false;
            if (SafetyOvercurrentInCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvercurrentInCharge: {SafetyOvercurrentInCharge}");
                result = false;
            }

            bool SafetyCellOvervoltageFailure = Convert.ToBoolean(myInt16Value & 0x0002) ? true : false;
            if (SafetyCellOvervoltageFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyCellOvervoltageFailure: {SafetyCellOvervoltageFailure}");
                result = false;
            }

            bool SafetyCellUndervoltageFailure = Convert.ToBoolean(myInt16Value & 0x0001) ? true : false;
            if (SafetyCellUndervoltageFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyCellUndervoltageFailure: {SafetyCellUndervoltageFailure}");
                result = false;
            }

            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParsePFAlert(): End of method");
            //return result;
            return myInt16Value;
        }


        //0x0053 PFStatus (32bits)
        //RSVD(Bit 31–18) : Reserved.Do not use.
        //DFW(Bit 17): Data Flash Wearout Failure (0x00020000)
        //IFC(Bit 16) : Instruction Flash Checksum Failure (0x00010000)
        //SOTF(Bit 15) : Safety Overtemperature FET Failure (0x00008000)
        //TS3(Bit 14) : Open Thermistor – TS3 Failure (0x00004000)
        //TS2(Bit 13) : Open Thermistor – TS2 Failure (0x00002000)
        //TS1(Bit 12) : Open Thermistor – TS1 Failure (0x00001000)
        //AFE_XRDY(Bit 11) : Companion BQ769x0 AFE XREADY Failure (0x00000800)
        //AFE_OVRD(Bit 10) : Companion BQ769x0 AFE Override Failure (0x00000400)
        //AFEC(Bit 9) : AFE Communication Failure (0x00000200)
        //AFER(Bit 8) : AFE Register Failure (0x00000100)
        //DFETF(Bit 7) : Discharge FET Failure (0x00000080)
        //CFETF(Bit 6) : Charge FET Failure (0x00000040)
        //VIMR(Bit 5): Voltage Imbalance while pack is at rest failure (0x00000020)
        //SOT(Bit 4) : Safety Overtemperature Cell Failure (0x00000010)
        //SOCD(Bit 3) : Safety Overcurrent in Discharge (0x00000008)
        //SOCC(Bit 2) : Safety Overcurrent in Charge (0x00000004)
        //SOV(Bit 1) : Safety Cell Overvoltage Failure (0x00000002)
        //SUV(Bit 0) : Safety Cell Undervoltage Failure (0x00000001)
        private UInt32 ParsePFStatus(string myPFStatusStr)
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParsePFStatus(): Start of method");
            bool result = false;


            UInt32 myInt32Value = Convert.ToUInt32(myPFStatusStr, 16);
            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParsePFStatus(): myInt32Value: {myInt32Value}");

            bool DataFlashWearoutFailure = Convert.ToBoolean(myInt32Value & 0x00020000) ? true : false;
            if (DataFlashWearoutFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): DataFlashWearoutFailure: {DataFlashWearoutFailure}");
                result = false;
            }

            bool InstructionFlashChecksumFailure = Convert.ToBoolean(myInt32Value & 0x00010000) ? true : false;
            if (InstructionFlashChecksumFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): InstructionFlashChecksumFailure: {InstructionFlashChecksumFailure}");
                result = false;
            }

            bool SafetyOvertemperatureFETFailure = Convert.ToBoolean(myInt32Value & 0x00008000) ? true : false;
            if (SafetyOvertemperatureFETFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvertemperatureFETFailure: {SafetyOvertemperatureFETFailure}");
                result = false;
            }

            bool OpenThermistorTS3Failure = Convert.ToBoolean(myInt32Value & 0x00004000) ? true : false;
            if (OpenThermistorTS3Failure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OpenThermistorTS3Failure: {OpenThermistorTS3Failure}");
                result = false;
            }

            bool OpenThermistorTS2Failure = Convert.ToBoolean(myInt32Value & 0x00020000) ? true : false;
            if (OpenThermistorTS2Failure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OpenThermistorTS2Failure: {OpenThermistorTS2Failure}");
                result = false;
            }

            bool OpenThermistorTS1Failure = Convert.ToBoolean(myInt32Value & 0x00001000) ? true : false;
            if (OpenThermistorTS1Failure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): OpenThermistorTS1Failure: {OpenThermistorTS1Failure}");
                result = false;
            }

            bool CompanionBQ769x0AFEXREADYFailure = Convert.ToBoolean(myInt32Value & 0x00000800) ? true : false;
            if (CompanionBQ769x0AFEXREADYFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CompanionBQ769x0AFEXREADYFailure: {CompanionBQ769x0AFEXREADYFailure}");
                result = false;
            }

            bool CompanionBQ769x0AFEOverrideFailure = Convert.ToBoolean(myInt32Value & 0x00000400) ? true : false;
            if (CompanionBQ769x0AFEOverrideFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): CompanionBQ769x0AFEOverrideFailure: {CompanionBQ769x0AFEOverrideFailure}");
                result = false;
            }

            bool AFECommunicationFailure = Convert.ToBoolean(myInt32Value & 0x00000200) ? true : false;
            if (AFECommunicationFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): AFECommunicationFailure: {AFECommunicationFailure}");
                result = false;
            }

            bool AFERegisterFailure = Convert.ToBoolean(myInt32Value & 0x00000100) ? true : false;
            if (AFERegisterFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): AFERegisterFailure: {AFERegisterFailure}");
                result = false;
            }

            bool DischargeFETFailure = Convert.ToBoolean(myInt32Value & 0x00000080) ? true : false;
            if (DischargeFETFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): DischargeFETFailure: {DischargeFETFailure}");
                result = false;
            }

            bool ChargeFETFailure = Convert.ToBoolean(myInt32Value & 0x00000040) ? true : false;
            if (ChargeFETFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): ChargeFETFailure: {ChargeFETFailure}");
                result = false;
            }

            bool VoltageImbalanceWhilePackIsAtRestFailure = Convert.ToBoolean(myInt32Value & 0x00000020) ? true : false;
            if (VoltageImbalanceWhilePackIsAtRestFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): VoltageImbalanceWhilePackIsAtRestFailure: {VoltageImbalanceWhilePackIsAtRestFailure}");
                result = false;
            }

            bool SafetyOvertemperatureCellFailure = Convert.ToBoolean(myInt32Value & 0x00000010) ? true : false;
            if (SafetyOvertemperatureCellFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvertemperatureCellFailure: {SafetyOvertemperatureCellFailure}");
                result = false;
            }

            bool SafetyOvercurrentInDischarge = Convert.ToBoolean(myInt32Value & 0x00000008) ? true : false;
            if (SafetyOvercurrentInDischarge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvercurrentInDischarge: {SafetyOvercurrentInDischarge}");
                result = false;
            }

            bool SafetyOvercurrentInCharge = Convert.ToBoolean(myInt32Value & 0x00000004) ? true : false;
            if (SafetyOvercurrentInCharge)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyOvercurrentInCharge: {SafetyOvercurrentInCharge}");
                result = false;
            }

            bool SafetyCellOvervoltageFailure = Convert.ToBoolean(myInt32Value & 0x00000002) ? true : false;
            if (SafetyCellOvervoltageFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyCellOvervoltageFailure: {SafetyCellOvervoltageFailure}");
                result = false;
            }

            bool SafetyCellUndervoltageFailure = Convert.ToBoolean(myInt32Value & 0x00000001) ? true : false;
            if (SafetyCellUndervoltageFailure)
            {
                _log.Log(consoleColor, $"TRSocketIPsViewModel::ParseSafetyAlert(): SafetyCellUndervoltageFailure: {SafetyCellUndervoltageFailure}");
                result = false;
            }

            _log.Log(consoleColor, $"TRSocketIPsViewModel::ParsePFStatus(): End of method");
            //return result;
            return myInt32Value;
        }


        // in case of any error method should return an error message for the particular socket
        private TRSocketStateMessage CheckBatteryStatusAndAlarmsCommand()
        {
            _log.Log(consoleColor, $"TRSocketIPsViewModel::CheckBatteryStatusAndAlarmsCommand(): Start of method  ({this.GetHashCode():x8})");

            TRSocketStateMessage trssm = new();
            trssm.MyStateName = "CheckBatteryStatusAndAlarmsCommand";
            //Dictionary<IntPtr, Tuple<SafetyAlertEnum, SafetyStatusEnum, PFAlertEnum, PFStatusEnum>> batteryStatusAndAlarmsDict = new();
            Dictionary<IntPtr, Tuple<UInt16, UInt16, UInt32, UInt32, UInt16, UInt32>> batteryStatusAndAlarmsDict = new();

            if (_myListOfSockets.Count == 0)
            {
                batteryStatusAndAlarmsDict.Add((IntPtr)0, new Tuple<UInt16, UInt16, UInt32, UInt32, UInt16, UInt32>(0, 0, 0, 0, 0, 0));
            }

            foreach (Socket terminalSocket in _myListOfSockets)
            {
                string myResponse37_BMS_REG = this.SendToSocket(_myListOfSockets[0], ParseOutputData("37/BMS/REG"));
                //_log.Log(consoleColor, $"TRSocketIPsViewModel::CheckBatteryStatusAndAlarmsCommand(): myResponse37_BMS_REG:\n{myResponse37_BMS_REG}");
                string myBatteryMode = this.ParseResponseString(myResponse37_BMS_REG, "Battery Mode");
                _log.Log(consoleColor, $"TRSocketIPsViewModel::CheckBatteryStatusAndAlarmsCommand(): myBatteryMode: {myBatteryMode}");
                UInt16 BatteryMode = this.ParseBatteryMode(myBatteryMode);

                string myBatteryStatus = this.ParseResponseString(myResponse37_BMS_REG, "Battery Status");
                UInt16 BatteryStatus = this.ParseBatteryStatus(myBatteryStatus);

                string mySafetyAlertStr = this.ParseResponseString(myResponse37_BMS_REG, "Safety Alert");
                UInt32 SafetyAlert = this.ParseSafetyAlert(mySafetyAlertStr);

                string mySafetyStatusStr = this.ParseResponseString(myResponse37_BMS_REG, "Safety Status");
                UInt32 SafetyStatus = this.ParseSafetyStatus(mySafetyStatusStr);

                string myPFAlertStr = this.ParseResponseString(myResponse37_BMS_REG, "PF Alert");
                UInt16 PFAlert = this.ParsePFAlert(myPFAlertStr);

                string myPFStatusStr = this.ParseResponseString(myResponse37_BMS_REG, "PF Status");
                UInt32 PFStatus = this.ParsePFStatus(myPFStatusStr);

                //if ((SafetyAlert) && (SafetyStatus) && (PFAlert) && (PFStatus))
                //    _log.Log(consoleColor, $"TRSocketIPsViewModel::CheckBatteryStatusAndAlarmsCommand(): Status is OK");
                //else
                //    _log.Log(consoleColor, $"TRSocketIPsViewModel::CheckBatteryStatusAndAlarmsCommand(): Status is not OK");

                Tuple<UInt16, UInt16, UInt32, UInt32, UInt16, UInt32> MyTuple = new(BatteryMode, BatteryStatus, SafetyAlert, SafetyStatus, PFAlert, PFStatus);
                batteryStatusAndAlarmsDict.Add(terminalSocket.Handle, MyTuple);
            }

            trssm.BatteryStatusAndAlarmsDict = batteryStatusAndAlarmsDict;
            _log.Log(consoleColor, $"TRSocketIPsViewModel::CheckBatteryStatusAndAlarmsCommand(): End of method  ({this.GetHashCode():x8})");
            return trssm;
        }



        #region Constructor
        public TRSocketIPsViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketIPsViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;

            _connectionItems = new List<ConnectionItem>();
            ConnectionItem _connectionItem1 = new ConnectionItem { Name = "TR1", Port = 42022, Host = "10.239.27.140" };    // 010.239.027.140
            _connectionItems.Add(_connectionItem1);
            ConnectionItem _connectionItem2 = new ConnectionItem { Name = "TR2", Port = 42022, Host = "10.239.27.141" };
            _connectionItems.Add(_connectionItem2);
            ConnectionItem _connectionItem3 = new ConnectionItem { Name = "TR3", Port = 42022, Host = "10.239.27.142" };
            //_connectionItems.Add(_connectionItem3);
            ConnectionItem _connectionItem4 = new ConnectionItem { Name = "TR4", Port = 42022, Host = "10.239.27.143" };
            //_connectionItems.Add(_connectionItem4);

            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketIPsViewModel(): _connectionItems.Count: {_connectionItems.Count}");



            _messenger.Register<TRSocketIPsViewModel, TRSocketInitRequestMessage>(this, (myReceiver, myMessenger) =>
             {
                 // musi zwracac wszyskie podlonczone sokety do ProductionViewModel
                 myMessenger.Reply(myReceiver.TRSocketInitAsync());
             });

            _messenger.Register<TRSocketIPsViewModel, TRShutdownRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.RunTRShutdownCommand());       // pacz ShellViewModel::IsShuttingDown
            });

            _messenger.Register<TRSocketIPsViewModel, TRSocketCheckPowerSupplyRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.TRSocketCheckPowerSupplyCommand());       // pacz ShellViewModel::IsShuttingDown
            });


            _messenger.Register<TRSocketIPsViewModel, CheckBatteryStatusAndAlarmsRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.CheckBatteryStatusAndAlarmsCommand());
            });



            _log.Log(consoleColor, $"TRSocketIPsViewModel::TRSocketIPsViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }
        #endregion Constructor

    }
}