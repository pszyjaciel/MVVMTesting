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

        private MyUser _myETUser;
        public MyUser myETUser
        {
            get
            {
                return _myETUser;
            }
            set
            {
                _myETUser = value;
            }
        }


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
        private object m_dteSolution;

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

            //_messenger.Register<ResetMessage>(this, (r, m) =>
            //{
            //    // do your resetting here
            //    _log.Log($"EastTesterViewModel::EastTesterViewModel(1): r: {r}, m: {m}");
            //    _log.Log($"EastTesterViewModel::EastTesterViewModel(1): r: {r}, m: {m}");
            //});

            //_messenger.Register<OperationMessage, bool>(this, MessageOp.Exit, (r, m) =>
            //{
            //    // do your resetting here
            //    _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(2): r: {r}, m: {m}");
                
                
            //});


            //_messenger.Register<OperationMessage, MessageOp>(this, MessageOp.Reset, (r, m) =>
            //{
            //    // do your resetting here
            //    _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(2): r: {r}, m: {m}");
            //});

            MessageOp mo1 = new MessageOp("myReset");
            //_messenger.Register<ResetMessage, MessageOp>(this, mo1, (r, m) =>
            //    {
                    
            //    });

            //_messenger.Register<OperationMessage>(this, (r, m) => {
            //    // handle all operations here - the operation is avaiable via m.Content
            //});

            //_messenger.Register<MessageOp>(this, (m) => {
            //    if (m == MessageOp.Reset)
            //    {
            //        // your code
            //    }
            //});



            _messenger.Register<MyObject>(this, (r, m) =>
            {
                // do your resetting here
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(3): r: {r}, m: {m}");
            });

            _messenger.Register<MyObjectChangedMessage>(this, (r, m) =>
            {
                // do your resetting here
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(4): r: {r}, m: {m}");
            });

            _messenger.Register<MyPerson>(this, (r, m) =>
            {
                // do your resetting here
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(5): r: {r}, m: {m}");
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(5): r: {r}, m: {m.FirstName}");
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(5): r: {r}, m: {m.LastName}");
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(5): r: {r}, m: {m.PersonsAge}");
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(5): r: {r}, m: {m.Neutralise}");

            });

            LoggedInUserRequestMessage liurm = null;
            try
            {
                liurm = WeakReferenceMessenger.Default.Send<LoggedInUserRequestMessage>();
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): liurm: {liurm}");
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): liurm.HasReceivedResponse: {liurm.HasReceivedResponse}");

            }
            catch (Exception ex)
            {
                _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): {ex.Message}");
            }


            //Task myTask = Task.Run(MyTask);
            //myTask.Wait();

            //_log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): liurm.HasReceivedResponse: {liurm.HasReceivedResponse}");
            //_log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): liurm.Response: {liurm.Response}");


            //MyUser myUser = WeakReferenceMessenger.Default.Send<LoggedInUserRequestMessage>();


            ///////////////////////////////
            //NotificationMessageAction<MessageBoxResult> msg = new NotificationMessageAction<MessageBoxResult>(this, "GetPassword", (r) =>
            //{
            //    if (r.Equals(MessageBoxResult.IDOK))
            //    {
            //        // do stuff
            //        _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): sie ruwna.");
            //    }
            //});

            ShowPasswordMessage msg = new ShowPasswordMessage(this, (r) =>
            {
                _log.Log(consoleColor, $"==> EastTesterViewModel::EastTesterViewModel(): r: {r}");

                if (r.Equals(MessageBoxResult.IDOK))
                {
                    // do stuff
                    _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): sie ruwna.");
                }
            });
            _messenger.Send(msg);
            _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): mesecz pojszet.");

            //_log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): myUser._myName: {myUser._myName}");

            _myEastTesterPrivateProperyName = new Post { Title = "EastTesterOldTitle2", Thumbnail = "EastTesterOldThumbnail2", SelfText = "Some old EastTester text2" };
            _messenger.Send(new PropertyChangedPostMessage(this, "MyEastTesterPublicProperyName", _myEastTesterPrivateProperyName,
                new Post { Title = "EastTesterTitle2", Thumbnail = "EastTesterThumbnail2", SelfText = "Some EastTester text2" }));




            _messenger.Register<CasualtyMessage, bool>(this, false, (r, m) => { RunBlanketStatusFalse(); });
            _messenger.Register<CasualtyMessage, bool>(this, true, (r, m) => { RunBlanketStatusTrue(); });

            _messenger.Send<MyTestMessage<MyEnum>>();       // gdy stont wysle to recipient nie reaguje, mimo rze dziala z sendera

            _log.Log(consoleColor, $"EastTesterViewModel::EastTesterViewModel(): end of constructor  ({this.GetHashCode():x8})");
        }


        private void RunBlanketStatusTrue()
        {
            _log.Log(consoleColor, $"EastTesterViewModel::RunBlanketStatusTrue()  ({this.GetHashCode():x8})");
        }

        private void RunBlanketStatusFalse()
        {
            _log.Log(consoleColor, $"EastTesterViewModel::RunBlanketStatusFalse()  ({this.GetHashCode():x8})");

        }



        private async Task MyTask()
        {
            _log.Log(consoleColor, $"LCSocketViewModel::MyTask()");
            await Task.Delay(2000);
        }
    }
}