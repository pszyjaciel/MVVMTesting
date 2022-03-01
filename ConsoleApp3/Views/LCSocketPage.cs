
using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;

namespace Console_MVVMTesting.Views
{
    internal class LCSocketPage : MyPage
    {
        public LCSocketViewModel XamlLCSocketViewModel { get; private set; }
        public EastTesterViewModel XamlEastTesterViewModel { get; private set; }
        private MyUtils mu = new MyUtils();

        public LCSocketPage()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"LCSocketPage::LCSocketPage() " +
              $"({this.GetHashCode():x8})");

            XamlLCSocketViewModel = Ioc.Default.GetService<LCSocketViewModel>();
            XamlEastTesterViewModel = Ioc.Default.GetService<EastTesterViewModel>();
            
        }
    }
}