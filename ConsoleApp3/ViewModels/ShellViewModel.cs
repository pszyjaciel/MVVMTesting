using Console_MVVMTesting.Helpers;
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
    public class ShellViewModel : ObservableObject
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;

        public ShellViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log($"ShellViewModel::ShellViewModel() ({this.GetHashCode():x8})");

            _messenger = messenger;
        }
    }
}
