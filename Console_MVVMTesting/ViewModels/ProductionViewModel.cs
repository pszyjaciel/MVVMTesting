using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
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
    class ProductionViewModel : ObservableRecipient
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;

        private const string consoleColor = "LBLUE";


        #region XamlIsSocketInitialized1
        private bool isSocketInitialized1;
        public bool XamlIsSocketInitialized1
        {
            get { return isSocketInitialized1; }
            set { SetProperty(ref isSocketInitialized1, value); }
        }
        #endregion


        #region TestRegex
        private void TestRegex()
        {
            _log.Log(consoleColor, $"ProductionViewModel::TestRegex()");

            string heartbeat = "'0x40''0x0d''0x0a'";    // litosci!
            //_log.Log(consoleColor, $"{heartbeat}");

            string heartbeat1 = "@\r\n";
            _log.Log(consoleColor, $"{heartbeat1}");
            string heartbeat2 = "@\n\r";
            _log.Log(consoleColor, $"{heartbeat1}");

            byte[] myBytes = { 0x40, 0x0d, 0x0a, 0x40, 0x0d, 0x0a, 0x40, 0x0d, 0x0a, 0x40, 0x0d, 0x0a, 0x43, 0x44,
                0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x0d, 0x0a, 0x00 };
            char[] cArray = System.Text.Encoding.ASCII.GetString(myBytes).ToCharArray();

            string response = new string(cArray);
            MyUtils.DisplayStringInBytes(response);
            MyUtils.DisplayStringInBytes(response.Replace(heartbeat1, ""));
            MyUtils.DisplayStringInBytes(response.Replace(heartbeat2, ""));

        }
        #endregion TestRegex



        public MyUser GetCurrentUserAsync()
        {
            return new MyUser("from ProductionViewModel(): huj w dupe i kula wuep putinowi");
        }



        #region Constructor
        public ProductionViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;

            //this.TestRegex();

            //Task.Delay(2000);
            //_messenger.Send(new InitETMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): InitETMessage()  ({GetHashCode():x8})");

            //_messenger.Send(new LCInitMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): LCInitMessage()  ({GetHashCode():x8})");

            //_messenger.Send(new LCCloseMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): LCCloseMessage()  ({GetHashCode():x8})");


            // request value from LCSocketViewModel
            MyUser myUser = _messenger.Send<LoggedInUserRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): myUser._myName: {myUser.MyUserName}");


            // Register a message in some module
            _messenger.Register<LoggedInUserChangedMessage>(this, (r, m) =>
            {
                // Handle the message here, with r being the recipient and m being the
                // input messenger. Using the recipient passed as input makes it so that
                // the lambda expression doesn't capture "this", improving performance.
               
                _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel():  r.GetType(): { r.GetType()}; m.Value: {m.Value}");
                
            });




            // C:\Users\pak\Source\Repos\MVVM-Samples-master\samples\MvvmSampleUwp.sln
            _messenger.Register<LCSocketStateMessage>(this, (r, m) =>
            {
                if (m.LCStatus == LCSocketStatusEnum.Connected)
                {
                    XamlIsSocketInitialized1 = true;
                }
                else
                {
                    XamlIsSocketInitialized1 = false;
                }
                _log.Log($"ProductionViewModel::ProductionViewModel(): XamlIsSocketInitialized1 is {XamlIsSocketInitialized1}");

                m.Reply(m.LCStatus);
            });

            Task.WaitAll();
            _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }
        #endregion
    }
}