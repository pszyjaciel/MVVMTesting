using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static PInvoke.User32;


// immediate window:
// >Edit.ClearAll
// >cls


namespace Console_MVVMTesting.ViewModels
{
    public class EastTesterViewModel : ObservableObject
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;
        private bool _isBusy;
        private const string consoleColor = "DCYAN";


        public bool IsBusy
        {
            get
            {
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel():IsBusy.get: {_isBusy}");
                return _isBusy;
            }
            private set
            {
                _isBusy = value;
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel():IsBusy.set: {_isBusy}");

            }
        }

        private bool _isDateTime;
        public bool IsDateTime
        {
            get
            {
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel():IsDateTime.get: {_isDateTime}");
                return _isDateTime;
            }
            private set
            {
                _isDateTime = value;
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel():IsDateTime.set: {_isDateTime}");

            }
        }


        #region XamlBatteryType
        private string _batteryType;
        public string XamlBatteryType
        {
            get { return _batteryType; }
            set { SetProperty(ref _batteryType, value); }
        }
        #endregion XamlBatteryType

        #region XamlNumberOfSets
        private int _numberOfSets;
        public int XamlNumberOfSets
        {
            get { return _numberOfSets; ; }
            set { SetProperty(ref _numberOfSets, value); }
        }
        #endregion XamlNumberOfSets


        private Post _myEastTesterPrivateProperyName;
        public Post MyEastTesterPublicProperyName
        {
            get => _myEastTesterPrivateProperyName;
            set => SetProperty(ref _myEastTesterPrivateProperyName, value, "");
        }


        public int MyEastTesterViewModelMethod(int somePrimitiveIntVariable)
        {
            _log.Log(consoleColor, $"EastTesterViewModel::MyEastTesterViewModelMethod(): I am in the MyEastTesterViewModelMethod()");

            somePrimitiveIntVariable++;
            return somePrimitiveIntVariable;
        }


        private bool RunInitCommandMessage()
        {
            _log.Log(consoleColor, $"EastTesterViewModel::RunInitCommandMessage(): Start of method  ({this.GetHashCode():x8})");

            bool initResult = true;
            // synchronous, because of parallel threads
            Task myOpenPortTask = Task.Run(() =>
            {
                //_myListOfSerialPorts = this.RetrieveSerialPortParallel();    // tu sie fszysko dzieje
                //_log.Log($"EastTesterViewModel::RunInitCommandMessage(): myListOfSerialPorts.Count: {_myListOfSerialPorts.Count}");
                //if (_myListOfSerialPorts.Count == 0)
                //{
                //    initResult = false;
                //}

                //this.InitLinkGroupColors(_myListOfSerialPorts);
                //this.InitMeasureThreads(_myListOfSerialPorts);
                //this.InitAttachDetachButtons(_myListOfSerialPorts);
            });
            myOpenPortTask.Wait();

            _log.Log(consoleColor, $"EastTesterViewModel::RunInitCommandMessage(): End of method; initResult: {initResult}  ({this.GetHashCode():x8})");
            return initResult;
        }


        private async Task<EastTesterStateMessage> EasyTesterInitAsync()
        {
            _log.Log(consoleColor, $"EastTesterViewModel::EasyTesterInitAsync()");

            bool rs = await Task.Run(RunInitCommandMessage);
            return new EastTesterStateMessage { MyStateName = "EastTesterViewModel", etStatus = rs ? ETStatus.Success : ETStatus.Error, ETErrorNumber = 0 };
        }


        private void SendLoggedInUserChangedMessage()
        {
            _log.Log(consoleColor, "EastTesterViewModel::SendLoggedInUserChangedMessage(): Start of method");
            AutoResetEvent myWait = new AutoResetEvent(false);
            myWait.Set();

            Task.Delay(2300);

            //MyUser myUser = new MyUser { MyUserName = "EastTesterUserName" };
            // Send a message from some other module
            //_messenger.Send(new LoggedInUserChangedMessage(myUser));
            myWait.WaitOne();

            _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): _messenger.Send(new LoggedInUserChangedMessage(myUser))");
            _log.Log(consoleColor, "EastTesterViewModel::SendLoggedInUserChangedMessage(): End of method");
        }


        ////////////////// SHUTDOWN /////////////////////
        private bool RunPostCloseCommand()
        {
            _log.Log(consoleColor, $"EastTesterViewModel::RunPostCloseCommand(): Start of method");

            bool result = false, finalResult = true;

            _log.Log(consoleColor, $"EastTesterViewModel::RunPostCloseCommand(): End of method: finalResult: {finalResult}");
            return finalResult;
        }


        private EastTesterStateMessage RunETShutdownCommand()
        {
            _log.Log(consoleColor, $"EastTesterViewModel::RunETShutdownCommand()  Start of method");

            bool shutDownResult = false;
            Task MyClosingTask = Task.Run(() =>
            {
                shutDownResult = this.RunPostCloseCommand();
                _log.Log(consoleColor, $"EastTesterViewModel::RunETShutdownCommand(): shutDownResult: {shutDownResult}");
            });
            MyClosingTask.Wait();

            EastTesterStateMessage etsm = new EastTesterStateMessage();
            etsm.ETErrorNumber = shutDownResult ? 0 : -1;      // error number can expand
            etsm.MyStateName = "EastTesterViewModel";
            etsm.etStatus = shutDownResult ? ETStatus.Success : ETStatus.Error;

            _log.Log(consoleColor, $"EastTesterViewModel::RunETShutdownCommand()  End of method");
            return etsm;
        }


        private void NumberOfSetsValueChangedMessageHandler(EastTesterViewModel recipient, NumberOfSetsValueChangedMessage message)
        {
            _log.Log(consoleColor, $"EastTesterViewModel::NumberOfSetsValueChangedMessageHandler()  Start of method");

            XamlNumberOfSets = message.Value;
            _log.Log(consoleColor, $"EastTesterViewModel::NumberOfSetsValueChangedMessageHandler() XamlNumberOfSets: {XamlNumberOfSets}");

            _log.Log(consoleColor, $"EastTesterViewModel::NumberOfSetsValueChangedMessageHandler()  End of method");
        }

        private void BatteryTypeValueChangedMessageHandler(EastTesterViewModel recipient, BatteryTypeValueChangedMessage message)
        {
            _log.Log($"EastTesterViewModel::BatteryTypeValueChangedMessageHandler()  Start of method");

            //XamlBatteryType = message.Value

            _log.Log($"EastTesterViewModel::BatteryTypeValueChangedMessageHandler()  End of method");
        }



        #region Constructor
        public EastTesterViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;

            _messenger.Register<ShellStateMessage>(this, (r, m) =>
            {
                if (m.State == ShellState.BusyOn)
                {
                    IsBusy = true;
                }

                if (m.State == ShellState.BusyOff)
                {
                    IsBusy = false;
                }
                if (m.State == ShellState.DateTimeOn)
                {
                    IsDateTime = true;
                }

                if (m.State == ShellState.DateTimeOff)
                {
                    IsDateTime = false;
                }
                m.Reply(m.State);
            });


            // initialize load
            _messenger.Register<EastTesterViewModel, EastTesterInitRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.EasyTesterInitAsync());
            });


            // shutdown-listener
            _messenger.Register<EastTesterViewModel, EastTesterShutdownRequestMessage>(this, (myReceiver, myMessenger) =>
            {
                myMessenger.Reply(myReceiver.RunETShutdownCommand());       // pacz ShellViewModel::IsShuttingDown
            });


            _messenger.Register<EastTesterViewModel, BatteryTypeValueChangedMessage>(this, BatteryTypeValueChangedMessageHandler);
            _messenger.Register<EastTesterViewModel, NumberOfSetsValueChangedMessage>(this, NumberOfSetsValueChangedMessageHandler);



            _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): end of constructor  ({this.GetHashCode():x8})");
        }
        #endregion Constructor



    }
}