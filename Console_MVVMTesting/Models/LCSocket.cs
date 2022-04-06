using Console_MVVMTesting.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Models
{
    internal class LCSocket
    {
        private readonly object consoleColor = "DWHITE";
        internal List<Socket> myListOfSockets { get; private set; }

        private ConnectionItem _connectionItem;
        private List<Tuple<Socket, IPAddress, ConnectionItem>> _connectionItemList;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent disconnectDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        internal IPAddress ipAddress { get; private set; }

        private const string _LC1Host = "10.239.27.140";
        private const string _LC2Host = "10.239.27.141";
        private const string _LC3Host = "10.239.27.142";
        private const string _LC4Host = "10.239.27.143";
        private const string _LC5Host = "10.239.27.144";
        private const string _LC6Host = "10.239.27.145";
        private const string _LC7Host = "10.239.27.146";
        private const string _LC8Host = "10.239.27.147";

  

        public bool IsConnected(Socket terminalSocket)
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::IsConnected(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

            bool result = (terminalSocket is not null) ? terminalSocket.Connected : false;
            return result;
        }



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
            //System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::IsHostnameValid(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

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
            //System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ResolveIp(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");
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



        #region Close
        /// <summary>
        /// Close method should be called when the Terminal connection is terminated.
        /// It closes the socket, and breaks out of the MessageHandler task.
        /// </summary>
        public void Close(Socket terminalSocket)    // pacz na _cancellationTokenSource1
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Close(): socket: { terminalSocket.Handle}");
            if (!IsConnected(terminalSocket)) return;

            try
            {
                terminalSocket.Shutdown(SocketShutdown.Both);    // Close the socket
                terminalSocket.Close();
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Close():  {se.SocketErrorCode}: {se.Message}");
            }
            catch (ObjectDisposedException ode)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Close():  {ode.ObjectName}: {ode.Message}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Close():  {e.HResult}: {e.Message}");
            }
            finally
            {
                //cts.Dispose();
                //_ctsDisposed1 = true;
            }
            //System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Close(): ThreadId: {Thread.CurrentThread.ManagedThreadId}  End of method.");
        }
        #endregion Close




        #region DisconnectCallback
        private void DisconnectCallback(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            System.Diagnostics.Debug.WriteLine(consoleColor, "LCSocketViewModel::DisconnectCallback(): Start of method");

            // Complete the disconnect request.
            Socket client = (Socket)ar.AsyncState;
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::DisconnectCallback(): socket: {client.Handle}");

            try
            {
                client.EndDisconnect(ar);  // Ends a pending asynchronous disconnect request.
                                           //client.Close();     // nie za bardzo..
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::DisconnectCallback: The terminal is disconnected!");
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::DisconnectCallback: {se.ErrorCode}: {se.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::DisconnectCallback: {ex.Message}");
            }
            // Signal that the disconnect is complete.
            disconnectDone.Set();
            System.Diagnostics.Debug.WriteLine(consoleColor, "LCSocketViewModel::DisconnectCallback(): End of method");
        }
        #endregion



        #region Disconnect
        // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.begindisconnect?view=net-6.0
        public bool Disconnect(Socket terminalSocket)
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Disconnect(): socket: {terminalSocket.Handle} - Start of method");
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
                        System.Diagnostics.Debug.WriteLine(consoleColor, "LCSocketViewModel::Disconnect(): We're still connected");
                        result = false;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(consoleColor, "LCSocketViewModel::Disconnect(): We're disconnected");
                        result = true;
                    }
                }
                catch (SocketException se)
                {
                    System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Disconnect(): {se.SocketErrorCode} : {se.Message}");
                    result = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Disconnect(): {ex.Message}");
                    result = false;
                }

            }
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::Disconnect(): End of method: result: {result}");
            return result;
        }
        #endregion




        #region SendCallBack
        private void SendCallBack(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            // Retrieve the socket from the state object.  
            Socket terminalSocket = (Socket)ar.AsyncState;
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendCallBack: socket: {terminalSocket.Handle}");

            try
            {
                // Complete sending the data to the remote device.  
                int bytesSend = terminalSocket.EndSend(ar);
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendCallBack: bytesSend: {bytesSend}");

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendCallBack: {ex.Message}");
            }
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion SendCallBack


        #region SendToSocket
        // BeginSend() Sends data asynchronously to a connected System.Net.Sockets.Socket.
        public string SendToSocket(Socket terminalSocket, string text)
        {
            if (terminalSocket == null) return String.Empty;
            if (IsConnected(terminalSocket) is false) return String.Empty;

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendToSocket(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

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
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendToSocket(): {se.NativeErrorCode} : {se.SocketErrorCode}: {se.Message}");
            }
            sendDone.WaitOne();

            // string response = this.ReceiveFromSocket(terminalSocket);

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::SendToSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
            return "";
        }
        #endregion SendToSocket




        #region ConnectCallback
        private void ConnectCallback(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            //System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectCallback(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            try
            {
                // Retrieve the socket from the state object.  
                Socket terminalSocket = (Socket)ar.AsyncState;

                // Complete the connection. Ends a pending asynchronous connection request.
                terminalSocket.EndConnect(ar); // throws an exception if connecting fails
                                               // Exception thrown: 'System.Net.Internals.SocketExceptionFactory.ExtendedSocketException' in System.Private.CoreLib.dll

                _connectionItem.ConnectTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectCallback(): socket: { terminalSocket.Handle} " +
                    $"connected to {terminalSocket.LocalEndPoint}");
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectCallback(): {se.SocketErrorCode} : {se.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectCallback(): {ex.Message}");
            }
            // Signal that the connection has been made.  (or not)
            connectDone.Set();
            //System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectCallback(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method.");
        }
        #endregion ConnectCallback



        #region MyConnectAsync
        private async Task<bool> MyConnectAsync(EndPoint remoteEP, Socket terminalSocket)
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::MyConnectAsync(): socket: {terminalSocket.Handle}, ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            IAsyncResult result = null;
            try
            {
                result = terminalSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), terminalSocket);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::MyConnectAsync(): {ex.Message}");
                return false;
            }

            // waiting for complition
            while (!result.IsCompleted)
            {
                await Task.Yield();
            }

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::MyConnectAsync(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method.");
            return (result is not null) ? result.IsCompleted : false;
        }
        #endregion MyConnectAsync



        #region ConnectToSocket
        public bool ConnectToSocket(Socket terminalSocket)
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectToSocket(): socket: {terminalSocket.Handle}, " +
                $"ThreadId: {Thread.CurrentThread.ManagedThreadId} : Start of method  ({this.GetHashCode():x8})");

            Task MyTask = Task.Run(async () =>
            {
                int connectTry = 3;    // how many try to connect?
                ipAddress = ResolveIp(_connectionItem.Host);
                EndPoint remoteEP = new IPEndPoint(ipAddress, _connectionItem.Port);
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

                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectToSocket(): {terminalSocket.RemoteEndPoint} --> {terminalSocket.LocalEndPoint}");
            });
            MyTask.Wait();

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ConnectToSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method");
            return IsConnected(terminalSocket);
        }
        #endregion ConnectToSocket



        // To cancel a pending BeginReceive one can call the Close method.
        #region ReceiveCallBack
        private void ReceiveCallBack(IAsyncResult ar)
        {
            string consoleColor = "LGREEN";     // kolbaki w innym kolorze
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - Start of method.");

            // Retrieve the state object and the client socket from the asynchronous state object.  
            StateObject so = (StateObject)ar.AsyncState;
            Socket terminalSocket = so.workSocket;

            string response = "", tmpResponse = "";
            string heartbeat = "@\r\n";
            try
            {
                // the socket server returns 0x0d for a new line, and 0x0d 0x0a for end of message
                int receivedBytes = terminalSocket.EndReceive(ar); //   Ends a pending asynchronous read.
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}, receivedBytes: {receivedBytes}");

                // Connection error occured, as the server never returns 0 bytes
                //if (receivedBytes == 0)
                //{
                //    System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): socket {terminalSocket.Handle}: It looks like a connection error!");
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
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): {se.NativeErrorCode} : {se.SocketErrorCode} : {se.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): {ex.Message}");
            }
            finally
            {
                receiveDone.Set();
            }
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveCallBack(): ThreadId: {Thread.CurrentThread.ManagedThreadId} - End of method.");
        }
        #endregion ReceiveCallBack



        // dopuszczalne tylko jedno wywolanie typu odpal i zapomnij
        #region ReceiveFromSocket
        public string ReceiveFromSocket(Socket terminalSocket)
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveFromSocket(): " +
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
                System.Diagnostics.Debug.WriteLine(consoleColor, e.ToString());
            }

            receiveDone.WaitOne();

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::ReceiveFromSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : End of method");
            return state.sb.ToString();
        }
        #endregion ReceiveFromSocket




        #region RunLCInitCommandMessage
        ///////////// INITIALIZING SOCKETS /////////////////
        // async doesn't work well with Parallel.ForEach: https://stackoverflow.com/a/23139769/7036047
        // The whole idea behind Parallel.ForEach() is that you have a set of threads and each thread processes part of the collection.
        // This doesn't work with async-await, where you want to release the thread for the duration of the async call.
        //private bool RunLCInitCommandMessage()
        internal async Task<LCSocketStateMessage> RunLCInitCommandMessage()
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            bool initResult = true;

            List<Socket> myListOfAvailableSockets = new List<Socket>();
            myListOfAvailableSockets.Add(new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            //myListOfAvailableSockets.Add(new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            //myListOfAvailableSockets.Add(new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            //myListOfAvailableSockets.Add(new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));

            int numberOfAvailableSockets = myListOfAvailableSockets.Count;
            int numberOfInitializedSockets = 0;
            myListOfSockets = new List<Socket>();

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
                    System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): Socket { mySocket.Handle} got response: {response}");

                    myListOfSockets.Add(mySocket);
                    numberOfInitializedSockets++;
                }
            });

            await Task.Delay(300);

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): myListOfSockets.Count: {myListOfSockets.Count}");


            if (numberOfInitializedSockets != numberOfAvailableSockets)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): not all sockets have been initialized");
                initResult = false;
            }

            Tuple<int, string> myTyple = new Tuple<int, string>(0, "");
            Dictionary<IntPtr, Tuple<int, string>> myDictionary = new Dictionary<IntPtr, Tuple<int, string>>();

            LCSocketStateMessage lcssm = new LCSocketStateMessage();
            lcssm.SocketInitDict = myDictionary;

            // we are ready with initialize
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::RunLCInitCommandMessage(): End of method with initResult: {initResult}  ({this.GetHashCode():x8})");
            return lcssm;
        }
        #endregion RunLCInitCommandMessage



        /// <summary>
        /// here we create a connection list with x-numberOfSets
        /// </summary>
        private void InitConnectionItemList(int numberOfSets)
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): Start of method");

            // _connectionItemList defined on top
            _connectionItemList = new List<Tuple<Socket, IPAddress, ConnectionItem>>();

            List<ConnectionItem> _connectionItems = new List<ConnectionItem>();
            ConnectionItem _connectionItem1 = new ConnectionItem { Name = "LC1", Port = 42026, Host = _LC1Host };    // cannot use 010.239.027.140
            _connectionItems.Add(_connectionItem1);
            ConnectionItem _connectionItem2 = new ConnectionItem { Name = "LC2", Port = 42026, Host = _LC2Host };
            _connectionItems.Add(_connectionItem2);
            ConnectionItem _connectionItem3 = new ConnectionItem { Name = "LC3", Port = 42026, Host = _LC3Host };
            _connectionItems.Add(_connectionItem3);
            ConnectionItem _connectionItem4 = new ConnectionItem { Name = "LC4", Port = 42026, Host = _LC4Host };
            _connectionItems.Add(_connectionItem4);
            ConnectionItem _connectionItem5 = new ConnectionItem { Name = "LC5", Port = 42026, Host = _LC5Host };
            _connectionItems.Add(_connectionItem5);
            ConnectionItem _connectionItem6 = new ConnectionItem { Name = "LC6", Port = 42026, Host = _LC6Host };
            _connectionItems.Add(_connectionItem6);
            ConnectionItem _connectionItem7 = new ConnectionItem { Name = "LC7", Port = 42026, Host = _LC7Host };
            _connectionItems.Add(_connectionItem7);
            ConnectionItem _connectionItem8 = new ConnectionItem { Name = "LC8", Port = 42026, Host = _LC8Host };
            _connectionItems.Add(_connectionItem8);

            //System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): _connectionItems.Count: {_connectionItems.Count}");

            switch (numberOfSets)
            {
                case 0:
                    _connectionItems.RemoveRange(0, 8);
                    break;
                case 1:
                    _connectionItems.RemoveRange(1, 7);
                    break;
                case 2:
                    _connectionItems.RemoveRange(2, 6);
                    break;
                case 3:
                    _connectionItems.RemoveRange(3, 5);
                    break;
                case 4:
                    _connectionItems.RemoveRange(4, 4);
                    break;
                case 5:
                    _connectionItems.RemoveRange(5, 3);
                    break;
                case 6:
                    _connectionItems.RemoveRange(6, 2);
                    break;
                case 7:
                    _connectionItems.RemoveRange(7, 1);
                    break;
                case 8:
                    _connectionItems.RemoveRange(8, 0);
                    break;

                default:
                    break;
            }

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): _connectionItems.Count: {_connectionItems.Count}");

            bool rs;
            Socket mySocket;
            LingerOption lingerOption = new LingerOption(true, 0);
            foreach (ConnectionItem ci in _connectionItems)
            {
                rs = IPAddress.TryParse(ci.Host, out IPAddress ipAddress);
                //System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): rs: {rs}");
                if (!rs)
                    return;

                mySocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 500,
                    ReceiveTimeout = 1000
                };
                mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);

                // ogulnie nie zalecajom urzywac reuse

                //mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                //What exactly does SO_REUSEADDR do?
                //This socket option tells the kernel that even if this port is busy (in
                //the TIME_WAIT state), go ahead and reuse it anyway.  If it is busy,
                //but with another state, you will still get an address already in use
                //error. It is useful if your server has been shut down, and then
                //restarted right away while sockets are still active on its port.  You
                //should be aware that if any unexpected data comes in, it may confuse
                //your server, but while this is possible, it is not likely.

                //It has been pointed out that "A socket is a 5 tuple (proto, local
                //addr, local port, remote addr, remote port).  SO_REUSEADDR just says
                //that you can reuse local addresses.The 5 tuple still must be
                //unique!" by Michael Hunter (mphunter@qnx.com).  This is true, and this
                //is why it is very unlikely that unexpected data will ever be seen by
                //your server.  The danger is that such a 5 tuple is still floating
                //around on the net, and while it is bouncing around, a new connection
                //from the same client, on the same system, happens to get the same
                //remote port.  This is explained by Richard Stevens in ``2.7 Please
                //explain the TIME_WAIT state.''.

                _connectionItemList.Add(new Tuple<Socket, IPAddress, ConnectionItem>(mySocket, ipAddress, ci));
            }

            // sprawdzenie
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): connectionItemList.Count: {_connectionItemList.Count}");
            foreach (Tuple<Socket, IPAddress, ConnectionItem> ci in _connectionItemList)
            {
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): ci.Item1.Handle: {ci.Item1.Handle}");
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): ci.Item2: {ci.Item2}");
                System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList(): ci.Item3.Name: {ci.Item3.Name}");
            }

            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList():  End of method");
        }


        public LCSocket(int numberSets)
        {
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList():  Start of constructor");
            this.InitConnectionItemList(numberSets);
            System.Diagnostics.Debug.WriteLine(consoleColor, $"LCSocketViewModel::InitConnectionItemList():  End of constructor");
        }



    }
}
