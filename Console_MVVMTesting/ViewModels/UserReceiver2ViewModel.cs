using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.ViewModels
{
    class UserReceiver2ViewModel : ObservableRecipient
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;
        private const string consoleColor = "LMAGENTA";

        private MyUser _currentUser;
        public MyUser CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }


        public UserReceiver2ViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"UserReceiver2ViewModel::UserReceiver2ViewModel(): Start of constructor ({this.GetHashCode():x8})");

            _messenger = messenger;

            // Register the receiver in a module; requested sender is in the ProductionViewModel
            // cannot replace UserReceiver2ViewModel with 'this'
            _messenger.Register<UserReceiver2ViewModel, LoggedInUserRequestMessage>(this, (r, m) =>
            {
                // Assume that "CurrentUser" is a private member in our viewmodel.
                // we're accessing it through the recipient passed as
                // input to the handler, to avoid capturing "this" in the delegate.
                //m.Reply(r._eastTesterViewModel.myETUser._myName);
                // Obs: this error can occure: A response has already been issued for the current message
                //m.Reply(r.GetCurrentUserAsync());       
            });


            //_messenger.Register<NotificationMessageAction<MessageBoxResult>>(this, (r, m) =>
            //{
            //    _log.Log($"UserReceiver2ViewModel::UserReceiver2ViewModel() m.Notification: {m.Notification}");

            //    if (m.Notification == "GetPassword")
            //    {
            //        PasswordDialog dlg = new PasswordDialog();
            //        object result = dlg.ShowDialog();
            //        m.Execute(result);
            //    }
            //});



            _currentUser = new MyUser("Pawel");


            // https://stackoverflow.com/questions/6440492/how-to-receive-dialogresult-using-mvvm-light-messenger

            _messenger.Register<ShowPasswordMessage>(this, (r, m) =>
            {
                PasswordDialog dlg = new PasswordDialog();
                string result = dlg.ShowDialog();
                m.Execute(result);
            });
            _messenger.Unregister<ShowPasswordMessage>(this);

            //_messenger.Register<UserReceiver2ViewModel, string>(this, _currentUser._myName);

            _messenger.Register<CasualtyMessage, bool>(this, false, (r, m) => { RunBlanketStatusFalse(); });
            _messenger.Register<CasualtyMessage, bool>(this, true, (r, m) => { RunBlanketStatusTrue(); });


            //_messenger.Register<OperationMessage>(this, (r, m) => { RunOperationMessage1(); });
            // _messenger.Send(new OperationMessage(MessageOp.Close));

            //_messenger.Register<InitMessage>((IRecipient)RunInitMessageDelegate);

            _messenger.Register<MyTestMessage<MyEnum>>(this, (r, m) =>
            {
                _log.Log($"UserReceiver2ViewModel::UserReceiver2ViewModel().MyTestMessage: r: {r}");  // Console_MVVMTesting.ViewModels.UserReceiver2ViewModel
                _log.Log($"UserReceiver2ViewModel::UserReceiver2ViewModel().MyTestMessage: m: {m}");  // [Console_MVVMTesting.Messages.MyEnum]
                RunMyTestMessage();
            });
            //_messenger.Register<InitETMessage>(this, (r, m) => { RunInitMessage(); });
            _messenger.Register<ResetMessage>(this, (r, m) => { RunResetMessage(); });
            _messenger.Register<OperationMessage<MessageOp>, bool>(this, MessageOp.Exit, (r, m) => { RunOperationMessage2(); });
            //_messenger.Register<ResetMessage<MessageOp>, bool>(this, MessageOp.Reset, (r, m) => { RunOperationMessage3(); });




            _log.Log(consoleColor, $"UserReceiver2ViewModel::UserReceiver2ViewModel(): End of constructor ({this.GetHashCode():x8})");
        }



        private void RunInitMessageDelegate()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunInitMessageDelegate()  ({this.GetHashCode():x8})");
        }

        private void RunMyTestMessage()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunMyTestMessage()  ({this.GetHashCode():x8})");
        }

        private void RunInitMessage()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunInitMessage()  ({this.GetHashCode():x8})");
        }

        private void RunResetMessage()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunResetMessage()  ({this.GetHashCode():x8})");
        }

        private void RunOperationMessage1()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunOperationMessage1()  ({this.GetHashCode():x8})");
        }

        private void RunOperationMessage2()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunOperationMessage2()  ({this.GetHashCode():x8})");
        }

        private void RunOperationMessage3()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunOperationMessage3()  ({this.GetHashCode():x8})");
        }



        private void RunBlanketStatusTrue()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunBlanketStatusTrue()  ({this.GetHashCode():x8})");
        }

        private void RunBlanketStatusFalse()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::RunBlanketStatusFalse()  ({this.GetHashCode():x8})");

        }

        private MyUser GetCurrentUserAsync()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::GetCurrentUserAsync()  ({this.GetHashCode():x8})");
            return new MyUser("from UserReceiver2ViewModel(): huj w dupe putinowi");
        }

        private async Task MyTask()
        {
            _log.Log(consoleColor, $"UserReceiver2ViewModel::MyTask()");
            await Task.Delay(10);
        }

    }
}
