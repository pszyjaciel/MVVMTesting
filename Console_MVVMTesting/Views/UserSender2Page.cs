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
    class UserSender2Page : MyPage
    {
        public UserSender2ViewModel XamlUserSender2ViewModel { get; private set; }

        public UserSender2Page()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"UserSender2Page::UserSender2Page() " +
               $"({this.GetHashCode():x8})");

            XamlUserSender2ViewModel = Ioc.Default.GetService<UserSender2ViewModel>();
        }
    }
}
