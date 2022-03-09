using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;


namespace Console_MVVMTesting.Views
{
    class UserReceiver2Page : MyPage
    {
        public UserReceiver2ViewModel XamlUserReceiver2ViewModel { get; private set; }
        

        public UserReceiver2Page()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] UserReceiverPage::UserReceiverPage() {this.GetHashCode()}");
            XamlUserReceiver2ViewModel = Ioc.Default.GetService<UserReceiver2ViewModel>();
        }
    }
}
