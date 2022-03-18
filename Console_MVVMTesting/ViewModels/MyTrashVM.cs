using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.ViewModels
{
    internal class MyTrashVM
    {
    }
}


// obs: parallel call:
// not really usefull with messenger, because too fast
//Parallel.ForEach(myListOfAvailableSockets, (mySocket) =>
//{
//    bool rs = false;
//    if (!IsConnected(mySocket))
//    {
//        rs = this.ConnectToSocket(mySocket);
//    }
//    if (rs)
//    {
//        ///// receiving the hello message /////
//        string response = this.ReceiveFromSocket(mySocket);
//        _log.Log(consoleColor, $"TRSocketViewModel::TRSocketInitAsync(): Socket { mySocket.Handle} got response: {response}");
//        _myListOfSockets.Add(mySocket);
//        Tuple<string, double, int> MyTuple = new(response, 0.0, TRErrorCode);   // can be simplified
//        MySocketDict.Add(mySocket.Handle, MyTuple);
//        trssm.MySocket = MySocketDict;
//        numberOfInitializedSockets++;
//    }
//});

///////////// INITIALIZING SOCKETS /////////////////
//private List<Socket> GetListOfAvailableSockets()
//{
//    _log.Log(consoleColor, $"LCSocketViewModel::GetListOfAvailableSockets(): Start of method");

//    List<Socket> ls = new List<Socket>();
//    ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
//    ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
//    ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
//    ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
//    ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
//    ls.Add(new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));

//    _log.Log(consoleColor, $"LCSocketViewModel::GetListOfAvailableSockets(): End of method");
//    return ls;
//}

// async doesn't work well with Parallel.ForEach: https://stackoverflow.com/a/23139769/7036047
// The whole idea behind Parallel.ForEach() is that you have a set of threads and each thread processes part of the collection.
// This doesn't work with async-await, where you want to release the thread for the duration of the async call.
//private void RunInitCommandMessage()
//{
//    _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): Start of method  ({this.GetHashCode():x8})");

//    List<Socket> myListOfAvailableSockets = GetListOfAvailableSockets();
//    foreach (Socket myAvailableSocket in myListOfAvailableSockets)
//    {
//        _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): socket: {myAvailableSocket.Handle}, " +
//            $"connected: {myAvailableSocket.Connected}");
//    }

//    bool rs = false;
//    _myListOfSockets = new List<Socket>();
//    Parallel.ForEach(myListOfAvailableSockets, (mySocket) =>
//    {
//        if (!IsConnected(mySocket))
//        {
//            rs = this.ConnectToSocket(mySocket);
//        }
//        if (rs)
//        {
//            _myListOfSockets.Add(mySocket);
//        }
//        rs = false;
//    });

//    _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): myListOfSockets.Count: {_myListOfSockets.Count}");
//    foreach (Socket myConnectedSocket in _myListOfSockets)
//    {
//        _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): socket: {myConnectedSocket.Handle}, " +
//            $"connected: {myConnectedSocket.Connected}");
//    }

//    _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): End of method  ({this.GetHashCode():x8})");
//}



// listen for the command in ProductionViewModel
//_messenger.Register<LCInitMessage>(this, (r, m) => { RunInitCommandMessage(); });
//_messenger.Register<LCCloseMessage>(this, (r, m) => { RunCloseCommandMessage(); });

///// PropertyChangedMessage /////
//Post myLCSocketPrivateProperyNameOld = new Post
//{
//    Updated = false,
//    Title = "LCSocketOldTitle2",
//    Thumbnail = "LCSocketOldThumbnail2",
//    SelfText = "Some old LCSocket text2"
//};

//Post myLCSocketPrivateProperyNameNew = new Post
//{
//    Updated = true,
//    Title = "LCSocketNewTitle",
//    Thumbnail = "LCSocketNewThumbnail",
//    SelfText = "Some new LCSocket text"
//};


//C:\Users\pak\Source\Repos\MVVM-Samples-master\samples\MvvmSampleUwp.sln

//_messenger.Send(new PropertyChangedPostMessage(this, nameof(MyLCSocketPublicProperyName),
//    myLCSocketPrivateProperyNameOld, myLCSocketPrivateProperyNameNew));

//LCSocketStateMessage lcssm = new LCSocketStateMessage(LCSocketStatusEnum.Connected);
//_messenger.Send(this, lcssm);     // wywala

//TMessage Send<TMessage, TToken>(TMessage message, TToken token)
//    where TMessage : class
//    where TToken : IEquatable<TToken>;

//_log.Log(consoleColor, $"LCSocketViewModel::LCSocketViewModel() XamlLCSocketViewModel.GetHashCode(): {XamlLCSocketViewModel.GetHashCode()}");



//#region RunInitCommandMessage2
//private void RunInitCommandMessage2()
//{
//    //_log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): Start of method  ({this.GetHashCode():x8})");

//    CancellationTokenSource myCancellationTokenSource1 = new CancellationTokenSource();
//    CancellationTokenSource myCancellationTokenSource2 = new CancellationTokenSource();
//    CancellationTokenSource myCancellationTokenSource3 = new CancellationTokenSource();
//    CancellationTokenSource myCancellationTokenSource4 = new CancellationTokenSource();

//    Task MyTask;

//    Socket terminalSocket1 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//    if (!IsConnected(terminalSocket1))
//    {
//        this.ConnectToSocket(terminalSocket1);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    Socket terminalSocket2 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//    if (!IsConnected(terminalSocket2))
//    {
//        this.ConnectToSocket(terminalSocket2);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    Socket terminalSocket3 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//    if (!IsConnected(terminalSocket3))
//    {
//        this.ConnectToSocket(terminalSocket3);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    Socket terminalSocket4 = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//    if (!IsConnected(terminalSocket4))
//    {
//        this.ConnectToSocket(terminalSocket4);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    ///// receiving /////
//    if (IsConnected(terminalSocket1))
//    {
//        this.ReceiveFromSocket(terminalSocket1);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    if (IsConnected(terminalSocket2))
//    {
//        this.ReceiveFromSocket(terminalSocket2);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    if (IsConnected(terminalSocket3))
//    {
//        this.ReceiveFromSocket(terminalSocket3);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    if (IsConnected(terminalSocket4))
//    {
//        this.ReceiveFromSocket(terminalSocket4);
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }


//    ///// sending /////
//    if (IsConnected(terminalSocket1))
//    {
//        this.SendToSocket(terminalSocket1, ParseOutputData("L10"));
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    if (IsConnected(terminalSocket2))
//    {
//        this.SendToSocket(terminalSocket2, ParseOutputData("L11"));
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    if (IsConnected(terminalSocket3))
//    {
//        this.SendToSocket(terminalSocket3, ParseOutputData("L12"));
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }

//    if (IsConnected(terminalSocket4))
//    {
//        this.SendToSocket(terminalSocket4, ParseOutputData("L13"));
//        MyTask = Task.Delay(250);
//        MyTask.Wait();
//    }


//    // now we want to close the open sockets
//    _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): waiting delay before we close all sockets..");
//    MyTask = Task.Delay(3000);
//    MyTask.Wait();

//    Task MyClosingTask = Task.Run(() =>
//       {
//           this.Close(terminalSocket1);
//           this.Close(terminalSocket2);
//           this.Close(terminalSocket3);
//           this.Close(terminalSocket4);

//           _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket1.Handle} connected: {IsConnected(terminalSocket1)}");
//           _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket2.Handle} connected: {IsConnected(terminalSocket2)}");
//           _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket3.Handle} connected: {IsConnected(terminalSocket3)}");
//           _log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): is socket {terminalSocket4.Handle} connected: {IsConnected(terminalSocket4)}");
//       });

//    MyClosingTask.Wait();   // wait before exit

//    //_log.Log(consoleColor, $"LCSocketViewModel::RunInitCommandMessage(): End of method.");
//}
//#endregion RunInitCommandMessage2

