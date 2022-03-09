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

        private const string consoleColor = "DCYAN";



        #region Constructor
        public ProductionViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;


            this.TestRegex();

            Task.Delay(2000);
            //_messenger.Send(new InitETMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): InitETMessage()  ({GetHashCode():x8})");

            //_messenger.Send(new InitLCMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): InitLCMessage()  ({GetHashCode():x8})");


            Task.WaitAll();
            _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }

        private void TestRegex()
        {
            _log.Log(consoleColor, $"ProductionViewModel::TestRegex()");

            string heartbeat = "'0x40''0x0d''0x0a'";    // zle
            //_log.Log(consoleColor, $"{heartbeat}");

            string heartbeat2 = "@\r\n";
            _log.Log(consoleColor, $"{heartbeat2}");

            byte[] myBytes = { 0x40, 0x0d, 0x0a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x0d, 0x0a, 0x00 };
            char[] cArray = System.Text.Encoding.ASCII.GetString(myBytes).ToCharArray();

            string response = new string(cArray);
            System.Diagnostics.Debug.WriteLine(response);
            MyUtils.DisplayStringInBytes(response);

            //foreach (char item in cArray)
            //{
            //    System.Diagnostics.Debug.Write((char)item);
            //}

            string tmpResponse = response.Replace(heartbeat2, "");
            System.Diagnostics.Debug.WriteLine(tmpResponse);
            MyUtils.DisplayStringInBytes(tmpResponse);

        }
        #endregion
    }
}