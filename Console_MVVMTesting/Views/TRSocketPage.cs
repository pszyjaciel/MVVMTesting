
using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;

namespace Console_MVVMTesting.Views
{
    internal class TRSocketPage : MyPage
    {
        //public TRSocketViewModel XamlTRSocketViewModel { get; private set; }
        public TRSocketIPsViewModel XamlTRSocketViewModel { get; private set; }
        

        public TRSocketPage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"TRSocketPage::TRSocketPage() " +
              $"({this.GetHashCode():x8})");

            XamlTRSocketViewModel = Ioc.Default.GetService<TRSocketIPsViewModel>();
            
        }
    }
}