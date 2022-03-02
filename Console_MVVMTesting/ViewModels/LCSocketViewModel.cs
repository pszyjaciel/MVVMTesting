using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static PInvoke.User32;


// Way-Way is observing me!
// Way-Way is observing me even more!

// (!)messages: https://social.technet.microsoft.com/wiki/contents/articles/30939.wpf-change-tracking.aspx

namespace Console_MVVMTesting.ViewModels
{
    public class LCSocketViewModel : ObservableObject
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;

        private readonly EastTesterViewModel _eastTesterViewModel;
        private const string consoleColor = "DGREEN";

        public ConnectionItem _connectionItem;
        private ConcurrentQueue<string> commandQueue;

        private Socket _terminalSocket = null;
        private Socket _terminalSocket1 = null;
        private Socket _terminalSocket2 = null;
        private Socket _terminalSocket3 = null;

        private IPAddress _ipAddress = null;
        private byte[] _receiveBuffer = null;
        private Int32 _connectRetryCounter = 0;

        private string _composedResponse;
        private bool _repetativeCmd = false;
        private bool _lastComposedCommand = false;
        private bool _composedCommand = false;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _ctsDisposed = false;
        private bool _closingDown = false;

        private AutoResetEvent ConnectedEvent = new AutoResetEvent(false);
        private AutoResetEvent ResponseReceivedEvent = new AutoResetEvent(false);
        private AutoResetEvent CommandInQueueEvent = new AutoResetEvent(false);
        private AutoResetEvent MessageHandlerTerminatedEvent = new AutoResetEvent(false);


        public bool IsConnected()
        {
            _log.Log($"LCSocketViewModel::IsConnected(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

            if (_terminalSocket is null)
            {
                _log.Log($"LCSocketViewModel::IsConnected(): _terminalSocket is null");
                return false;
            }

            bool connected = _terminalSocket.Connected;
            if (connected)
            {
                //_messenger.Send(new LCSocketStateMessage(LCSocketStatus.Connected));
                //_dispatcherQueue.TryEnqueue(() => { XamlBtnConnectSocketBackground = new SolidColorBrush(Colors.PaleGreen); });
            }
            else
            {
                //_messenger.Send(new LCSocketStateMessage(LCSocketStatus.Disconnected));
                //_dispatcherQueue.TryEnqueue(() => { XamlBtnConnectSocketBackground = new SolidColorBrush(Colors.LightPink); });
            }

            _log.Log($"LCSocketViewModel::IsConnected(): Socket status: {connected}");
            return connected;
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
            _log.Log($"LCSocketViewModel::IsHostnameValid(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

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


        // works in a new first-level Thread
        #region ResolveIp
        private IPAddress ResolveIp(string host)
        {
            _log.Log($"LCSocketViewModel::ResolveIp(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");
            IPAddress ip = null;

            if (IsHostnameValid(host))
            {
                if (IPAddress.TryParse(host, out IPAddress ipAddress) == false)
                {
                    // Resolve IP from hostname
                    var IPs = Dns.GetHostEntry(host).AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray(); // Keep only IPv4 in IPs array
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
        private string ParseResponseData(String data)
        {
            _log.Log($"LCSocketViewModel::ParseResponseData()");
            //_log.Log( $"LCSocketViewModel::ParseResponseData(): data: {data}");
            //MyUtils.DisplayStringInBytes(data);

            string parsedData = data;

            //40 0d 0a
            if (data.Length == 3)
            {
                if (((data[0] == 0x40) && (data[1] == 0x0a) && (data[2] == 0x0d))
                    || ((data[0] == 0x40) && (data[1] == 0x0d) && (data[2] == 0x0a)))
                {
                    parsedData = "[heartbeat]\n";
                }
            }

            //string parsedData = data;
            //string tmp = data;
            //do
            //{
            //    tmp = parsedData;
            //    //parsedData = parsedData.TrimEnd(new char[] { '\r', '\n', '@' });
            //    parsedData = parsedData.TrimEnd(new char[] { '@' });
            //}
            //while (parsedData != tmp);

            // this @ (0x40) can be treated as a heartbeat
            //_log.Log($"SocketViewModel::ParseResponseData(): parsedData:\n{parsedData}\n");
            //MyUtils.DisplayStringInBytes(parsedData);
            return parsedData;
        }
        #endregion ParseResponseData


        // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-an-asynchronous-client-socket
        // still a new Thread
        #region Socket Callbacks
        private void ReceiveCallBack(IAsyncResult ar)
        {
            _log.Log($"LCSocketViewModel::ReceiveCallBack1(): ThreadId: {Thread.CurrentThread.ManagedThreadId}");

            if (_closingDown)
                return;

            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                //Socket client = state.terminalSocket; // wywala

                int receivedBytes = _terminalSocket1.EndReceive(ar); //   Ends a pending asynchronous read.
                _log.Log($"LCSocketViewModel::ReceiveCallBack1(): receivedBytes: {receivedBytes }");
                if (receivedBytes == 0)
                {
                    string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    //Connect();
                    return; // disconnected?!?
                }

                // Handle received data
                Array.Resize(ref _receiveBuffer, receivedBytes);
                string text = Encoding.ASCII.GetString(_receiveBuffer);

                //Exception thrown: 'System.Runtime.InteropServices.COMException' in WinRT.Runtime.dll
                //LCSocketViewModel::ReceiveCallBack(): The application called an interface that was marshalled for a different thread. (0x8001010E (RPC_E_WRONG_THREAD))     

                //_dispatcherQueue.TryEnqueue(() => { XAMLtbReceiveSocketBox += text; });
                //MyUtils.DisplayStringInBytes(text);

                text = ParseResponseData(text);
        
                if (!String.IsNullOrEmpty(text))
                {
                    _composedResponse += "\r\n\r\n" + text;
                    ResponseReceivedEvent.Set();
                }
                Array.Resize(ref _receiveBuffer, _terminalSocket1.ReceiveBufferSize);

                // Get ready to receive new data (obs. callback is recursive function)
                _terminalSocket1.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);
            }
            catch (SocketException se)
            {
                _log.Log($"LCSocketViewModel::ReceiveCallBack1(): {se.Message}");
            }
            catch (ObjectDisposedException ode)
            {
                String msg = String.Format("{0}", ode.Message);
                _log.Log($"LCSocketViewModel::ReceiveCallBack1(): {ode.Message}");
            }
            catch (Exception ex)
            {
                _log.Log($"LCSocketViewModel::ReceiveCallBack1(): {ex.Message}");
            }
            _log.Log($"LCSocketViewModel::ReceiveCallBack1(): end of method.");
        }


        private void SendCallBack(IAsyncResult ar)
        {
            _log.Log("LCSocketViewModel::SendCallBack()");
            try
            {
                int bytesSend = _terminalSocket1.EndSend(ar);
            }
            catch (SocketException se)
            {
                _log.Log($"LCSocketViewModel::SendCallBack: {se.Message}");
            }
            catch (Exception ex)
            {
                _log.Log($"LCSocketViewModel::SendCallBack: {ex.Message}");
            }
        }
        #endregion Socket Callbacks


        // BeginSend() Sends data asynchronously to a connected System.Net.Sockets.Socket.
        // works in a second level new Thread!
        public void SendToSocket(string text)   // hujowata nazwa
        {
            _log.Log($"LCSocketViewModel::SendToSocket(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");
            try
            {
                if (_terminalSocket1 is not null)
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(text);
                    _terminalSocket1.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);
                }
            }
            catch (SocketException se)
            {
                _log.Log($"LCSocketViewModel::SendToSocket(): {se.SocketErrorCode}");
                _log.Log($"LCSocketViewModel::SendToSocket(): {se.Message}");
            }
        }


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


        #region MessageHandler
        /// <summary>
        /// The MessageHandler handles the commands. 
        /// When the user enters a command, it is put in the commandQueue and signaled (that there is a 
        /// command ready for execution). 
        /// The MessageHandler takes the command from the commandQueue, and sends it to the Unit (Bsc/TR/SB/...
        /// 
        /// The response is captured in the ReceiveCallback (in another thread), and signaled when its done.
        /// The MessageHandler waits for maximum the "CommandTimeout" period, if no response is received.
        /// 
        /// </summary>
        /// works in a second level new Thread!
        public async Task MessageHandler()
        {
            _log.Log($"LCSocketViewModel::MessageHandler(): ThreadId: {Thread.CurrentThread.ManagedThreadId}");

            CancellationToken ct = _cancellationTokenSource.Token;
            bool raiseCmdAndResp = true;
            string fullCommand = "";

            AutoResetEvent myWait = new AutoResetEvent(false);

            _log.Log($"LCSocketViewModel::MessageHandler(): ct.IsCancellationRequested: {ct.IsCancellationRequested} ");
            while (ct.IsCancellationRequested == false)
            {
                if (commandQueue.IsEmpty && (fullCommand.StartsWith(@"R/") || fullCommand.StartsWith(@"C/")))
                {
                    // CommandQueue is empty and last command shall be repeated, reuse fullCommand
                }
                else
                { // Pick new Command from CommandQueue
                    fullCommand = "";
                    try
                    {
                        // Exception thrown: 'System.Runtime.InteropServices.COMException' in WinRT.Runtime.dll
                        // som 3 sety i 5 waituf
                        CommandInQueueEvent.WaitOne();   // Wait for CommandInQueue is signaled
                        CommandInQueueEvent.Set();
                    }
                    catch (Exception e)
                    {
                        //_dispatcherQueue.TryEnqueue(() => { XAMLtbReceiveSocketBox += $"{e.Message} \n"; });    // wywala

                        //XAMLtbReceiveSocketBox += $"{e.Message} \n";
                        _log.Log($"LCSocketViewModel::MessageHandler: {e.Message}");
                    }

                    while (commandQueue.TryDequeue(out string cmdItem))
                    {
                        //XAMLtbReceiveSocketBox += cmdItem;
                        //MyUtils.DisplayStringInBytes(cmdItem);

                        if (raiseCmdAndResp)
                        {
                            //XAMLtbReceiveSocketBox += $"LCSocketViewModel::MessageHandler(): raiseCmdAndResp=={raiseCmdAndResp} \n";
                        }
                        if (String.IsNullOrEmpty(cmdItem) == false)
                        {
                            fullCommand = cmdItem;
                            break;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(fullCommand))
                {
                    string cmd = fullCommand;
                    _repetativeCmd = cmd.StartsWith(@"R/");
                    if (cmd.StartsWith(@"R/") || cmd.StartsWith(@"C/"))
                    {
                        cmd = cmd.Remove(0, 2);
                    }

                    string[] cmds = cmd.Split('|');
                    _lastComposedCommand = false;
                    _composedCommand = (cmds.Length > 1);
                    for (int i = 0; i < cmds.Length; i++)
                    {
                        if (i == (cmds.Length - 1))
                        {
                            _lastComposedCommand = true;
                        }
                        SendToSocket(ParseOutputData(cmds[i]));
                        try
                        {
                            //     Blocks the current thread until the current System.Threading.WaitHandle receives
                            //     a signal, using a 32-bit signed integer to specify the time interval in milliseconds.
                            ResponseReceivedEvent.WaitOne(2500);
                        }
                        catch (Exception e)
                        {
                            //XAMLtbReceiveSocketBox += $"{e.Message} \n";
                        }

                        await Task.Delay(1000);
                    }
                }
            } // end while (ct.IsCancellationRequested == false)
            MessageHandlerTerminatedEvent.Set();
            _log.Log($"LCSocketViewModel::MessageHandler(): ThreadId: {Thread.CurrentThread.ManagedThreadId}: end of method");
        }
        #endregion MessageHandler



        // still a new Thread
        private void ConnectCallback(IAsyncResult ar)
        {
            _log.Log($"LCSocketViewModel::ConnectCallback(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");
            try
            {
                //_log.Log($"LCSocketViewModel::ConnectCallback(): 1");

                //_terminalSocket = (Socket)ar.AsyncState;
                //if (_terminalSocket == null)
                //{
                //    _log.Log($"LCSocketViewModel::ConnectCallback(): _terminalSocket == null");
                //    return;
                //}

                // Ends a pending asynchronous connection request.
                _terminalSocket.EndConnect(ar); // throws an exception if connecting fails
                                                // Exception thrown: 'System.Net.Internals.SocketExceptionFactory.ExtendedSocketException' in System.Private.CoreLib.dll

                _connectionItem.ConnectTime = DateTime.Now;
                _log.Log($"LCSocketViewModel::ConnectCallback(): Connected to {_ipAddress.ToString()}:{_connectionItem.Port} {_connectionItem.ConnectTime}");

                //_messenger.Send(new LCSocketStateMessage(LCSocketStatus.Connected));

                _terminalSocket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);

                // Blocks the current thread until the current System.Threading.WaitHandle receives a signal.
                ResponseReceivedEvent.WaitOne(); // Wait for "Welcome" message

                Task MyTask = Task.Run(async () =>
                {
                    await MessageHandler();   // tu nie wywala (na 100%)
                }, _cancellationTokenSource.Token);

                _connectRetryCounter = 0;
            }
            catch (InvalidOperationException ioe)
            {
                _log.Log($"LCSocketViewModel::ConnectCallback1(): {ioe.Message}");
            }
            catch (SocketException se)
            {
                _log.Log($"LCSocketViewModel::ConnectCallback1(): {se.Message}");
            }
            catch (Exception ex)
            {
                _log.Log($"LCSocketViewModel::ConnectCallback1(): {ex.Message}");
            }

            // Exception thrown: 'System.Runtime.InteropServices.COMException' in WinRT.Runtime.dll
            //ConnectedEvent.Set();     // to wywala na 100% gdy socket nie odpowiada
            //XAMLlblConnectionSocketStatus = $"Socket connected: {IsConnected()} \n";    // crashes here: wrong thread

            // jest jeszcze drugi error, pacz _terminalSocket.EndConnect(ar);
            // Exception thrown: 'System.Runtime.InteropServices.COMException' in System.Private.CoreLib.dll

            _log.Log($"LCSocketViewModel::ConnectCallback1(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : end of method");
        }


        #region Close
        /// <summary>
        /// Close method should be called when the Terminal connection is terminated.
        /// It closes the socket, and breaks out of the MessageHandler task.
        /// </summary>
        public void Close()
        {
            _log.Log("LCSocketViewModel::Close()");

            _closingDown = true;
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();              // Tell MessageHandler to cancel operation
            }
            CommandInQueueEvent.Set();    // MessageHandler may wait
            ResponseReceivedEvent.Set();  // for one of these events

            MessageHandlerTerminatedEvent.WaitOne(500);     // Is this Event really necessary???

            if (IsConnected())
            {
                try
                {
                    _terminalSocket1.Shutdown(SocketShutdown.Both);    // Close the socket
                    _terminalSocket1.Close();
                }
                catch (SocketException se)
                {
                    //XAMLtbReceiveSocketBox += $"{se.SocketErrorCode}: {se.Message} \n";
                }
                catch (ObjectDisposedException ode)
                {
                    //XAMLtbReceiveSocketBox += $"{ode.ObjectName}: {ode.Message} \n";
                }
                catch (Exception e)
                {
                    //XAMLtbReceiveSocketBox += $"{e.Message} \n";
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
                    _ctsDisposed = true;
                }
            }
            else
            {
                //XAMLtbReceiveSocketBox += $"Terminal is disconnected. \n";
                //XAMLConnectionSocketStatus = $"Socket connected: {IsConnected()} \n";
            }
            _log.Log("LCSocketViewModel::Close(): end of method");
        }
        #endregion Close


        // still a new Thread
        private void BeginConnect_old()
        {
            _log.Log($"LCSocketViewModel::BeginConnect(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");
            if (IsConnected() == true)
            {
                _log.Log("LCSocketViewModel::BeginConnect(): IsConnected() == true");
                Close();
            }
            _ipAddress = ResolveIp(_connectionItem.Host);
            _terminalSocket1 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);



            _log.Log($"LCSocketViewModel::BeginConnect(): _ipAddress: {_ipAddress}");
            byte[] myAddressBytes = _ipAddress.GetAddressBytes();
            _log.Log($"LCSocketViewModel::BeginConnect(): {myAddressBytes[0]}:{myAddressBytes[1]}:{myAddressBytes[2]}:{myAddressBytes[3]}");
            _log.Log($"LCSocketViewModel::BeginConnect(): _ipAddress.AddressFamily: {_ipAddress.AddressFamily}");

            _terminalSocket1.ReceiveTimeout = 2000;
            _receiveBuffer = new byte[_terminalSocket1.ReceiveBufferSize];

            IAsyncResult result = _terminalSocket1.BeginConnect(new IPEndPoint(_ipAddress, _connectionItem.Port), new AsyncCallback(ConnectCallback), null);


            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket: {_terminalSocket}");
            _log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.RemoteEndPoint: {_terminalSocket1.RemoteEndPoint}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.Handle: {_terminalSocket.Handle}");
            _log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.LocalEndPoint: {_terminalSocket1.LocalEndPoint}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.SocketType: {_terminalSocket.SocketType}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.ProtocolType: {_terminalSocket.ProtocolType}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.Available: {_terminalSocket.Available}");

            _log.Log($"LCSocketViewModel::BeginConnect(): result: {result}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): result.AsyncState: {result.AsyncState}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): result.CompletedSynchronously: {result.CompletedSynchronously}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): result.AsyncWaitHandle: {result.AsyncWaitHandle}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): result.IsCompleted: {result.IsCompleted}");

            if (result.IsCompleted)
            {
                // kolorki dzialajom, to nie tu jest kucha
                //_dispatcherQueue.TryEnqueue(() => { XamlBtnConnectSocketBackground = new SolidColorBrush(Colors.PaleGreen); });
            }
            else
            {
                //_dispatcherQueue.TryEnqueue(() => { XamlBtnConnectSocketBackground = new SolidColorBrush(Colors.LightPink); });
            }

            //_terminalSocket2 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //result = _terminalSocket2.BeginConnect(new IPEndPoint(_ipAddress, _connectionItem.Port), new AsyncCallback(ConnectCallback2), null);
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.RemoteEndPoint: {_terminalSocket2.RemoteEndPoint}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.LocalEndPoint: {_terminalSocket2.LocalEndPoint}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): result: {result}");

            //_terminalSocket3 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //result = _terminalSocket3.BeginConnect(new IPEndPoint(_ipAddress, _connectionItem.Port), new AsyncCallback(ConnectCallback3), null);
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.RemoteEndPoint: {_terminalSocket2.RemoteEndPoint}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): _terminalSocket.LocalEndPoint: {_terminalSocket2.LocalEndPoint}");
            //_log.Log($"LCSocketViewModel::BeginConnect(): result: {result}");


            _log.Log($"LCSocketViewModel::BeginConnect(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : end of method.");
        }


        private void BeginConnectAsync()
        {
            _log.Log($"LCSocketViewModel::BeginConnectAsync(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");
            if (IsConnected() == true)
            {
                Close();
            }
            _ipAddress = ResolveIp(_connectionItem.Host);
            _terminalSocket1 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _terminalSocket1.ReceiveTimeout = 2000;
            _receiveBuffer = new byte[_terminalSocket1.ReceiveBufferSize];

            IAsyncResult result = _terminalSocket1.BeginConnect(new IPEndPoint(_ipAddress, _connectionItem.Port), new AsyncCallback(ConnectCallback), null);


            _log.Log($"LCSocketViewModel::BeginConnectAsync(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : end of method.");
        }


        #region ConnectHandlerAsync
        /// <summary>
        /// ConnectHandlerAsync establishes a IP connection. 
        /// This handler pings the host to ensure it is active. If the ping is replyed, a connection attempt is tryed.
        /// IMPORTANT NOTE: You should NOT call this directly, instead use the public Connect() method.
        /// </summary>
        /// 
        /// ConnectHandlerAsync works in a new Thread!!
        private async Task ConnectHandlerAsync(CancellationToken ct)
        {
            _log.Log($"LCSocketViewModel::ConnectHandlerAsync(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

            // the difference CancellationTokenSource vs CancellationToken ??
            ct = _cancellationTokenSource.Token;

            _log.Log($"LCSocketViewModel::ConnectHandlerAsync(): ct.IsCancellationRequested: {ct.IsCancellationRequested}");
            _log.Log($"LCSocketViewModel::ConnectHandlerAsync(): IsConnected(): {IsConnected()}");

            // nie wyjdzie stont, dopuki sie nie pouonczy
            while ((ct.IsCancellationRequested == false) && (IsConnected() == false))
            {
                _log.Log($"LCSocketViewModel::ConnectHandlerAsync(): in while loop: before");

                this.BeginConnectAsync();
                ConnectedEvent.WaitOne();

                if (IsConnected() == false)
                {
                    await Task.Delay(10000);
                }

                _log.Log($"LCSocketViewModel::ConnectHandlerAsync(): in while loop: after");
            }
            _log.Log($"LCSocketViewModel::ConnectHandlerAsync(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : end of method.");
        }
        #endregion ConnectHandler



        #region Connect
        public void Connect()
        {
            _log.Log($"LCSocketViewModel::Connect(): ThreadId: {Thread.CurrentThread.ManagedThreadId}.");

            if ((_cancellationTokenSource is null) || _ctsDisposed == true)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _ctsDisposed = false;
            }

            Task MyTask = Task.Run(async () =>
            {
                await ConnectHandlerAsync(_cancellationTokenSource.Token);
            });

            _log.Log($"LCSocketViewModel::Connect(): ThreadId: {Thread.CurrentThread.ManagedThreadId} : end of method");
        }
        #endregion Connect


        //private void RunInitLCCommandMessage()
        //{
        //    // to ma byc w LCVM!
        //    //_messenger.Register<InitLCMessage>(this, (r, m) => { RunInitLCCommandMessage(); });

        //    _loggingService.Log(consoleColor, $"ProductionViewModel::RunInitCommandMessage()  ({this.GetHashCode():x8})");
        //    Task myOpenPortTask = Task.Run(() =>
        //    {
        //        this.MyInitTaskAsync();
        //    });

        //}



        private void RunInitCommandMessage()
        {
            _log.Log("LCSocketViewModel::RunInitCommandMessage(): Start of method");

            if (!IsConnected())
            {
                this.Connect();
            }

            _log.Log("LCSocketViewModel::RunInitCommandMessage(): End of method");
        }


        public LCSocketViewModel(ILoggingService loggingService, IMessenger messenger, EastTesterViewModel etvm)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel(): start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;
            _eastTesterViewModel = etvm;

            _connectionItem = new ConnectionItem();
            _connectionItem.Port = 42026;
            _connectionItem.Name = "LC-connection";
            _connectionItem.Host = "127.0.0.1";

            _ipAddress = new IPAddress(0);
            commandQueue = new ConcurrentQueue<string>();



            _messenger.Register<InitLCMessage>(this, (r, m) => { RunInitCommandMessage(); });

            //_log.Log($"LCSocketViewModel::LCSocketViewModel() XamlLCSocketViewModel.GetHashCode(): {XamlLCSocketViewModel.GetHashCode()}");
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel() etvm.GetHashCode(): {etvm.GetHashCode()}");
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel() etvm.IsBusy(): {etvm.IsBusy}");
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel() etvm.IsDateTime(): {etvm.IsDateTime}");
            int result = etvm.MyEastTesterViewModelMethod(12);
            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel() {result}");


            _log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel(): end of constructor  ({this.GetHashCode():x8})");
        }

        
    
        private void RunOperationMessage2()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::RunOperationMessage2()  ({this.GetHashCode():x8})");
        }



    }
}