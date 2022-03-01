using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Views
{
    class UserReceiverPage : MyPage
    {
        public UserReceiverViewModel XamlUserReceiverViewModel { get; private set; }
        private MyUtils mu = new MyUtils();

        public UserReceiverPage()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] UserReceiverPage::UserReceiverPage() {this.GetHashCode()}");
            XamlUserReceiverViewModel = Ioc.Default.GetService<UserReceiverViewModel>();
            XamlUserReceiverViewModel.IsActive = true;
        }
    }
}
