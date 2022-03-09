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
    class UserSenderPage : MyPage
    {
        public UserSenderViewModel XamlUserSenderViewModel { get; private set; }

        public UserSenderPage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"UserSenderPage::UserSenderPage() " +
               $"({this.GetHashCode():x8})");

            XamlUserSenderViewModel = Ioc.Default.GetService<UserSenderViewModel>();
        }
    }
}
