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
        private readonly ILoggingService _loggingService;
        private readonly IMessenger _messenger;

        private const string consoleColor = "DCYAN";



        #region Constructor
        public ProductionViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _loggingService = loggingService;
            _loggingService.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;


            Task.Delay(2000);
            //_messenger.Send(new InitETMessage());
            //_loggingService.Log($"ProductionViewModel::ProductionViewModel(): InitETMessage()  ({GetHashCode():x8})");

            _messenger.Send(new InitLCMessage());
            _loggingService.Log($"ProductionViewModel::ProductionViewModel(): InitLCMessage()  ({GetHashCode():x8})");


            _loggingService.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }
        #endregion
    }
}