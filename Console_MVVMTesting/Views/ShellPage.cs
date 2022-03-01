using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;

namespace Console_MVVMTesting.Views
{
    public class ShellPage : MyPage
    {
        public ShellViewModel XamlShellViewModel { get; }

        private MyUtils mu;

        public ShellPage()
        {
            mu = new MyUtils();

            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] ShellPage::ShellPage() {this.GetHashCode()}");
            XamlShellViewModel = Ioc.Default.GetService<ShellViewModel>();
     
        }
    }
}