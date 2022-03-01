using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;

namespace Console_MVVMTesting.Views
{
    internal class EastTesterPage : MyPage
    {
        public EastTesterViewModel XamlViewModel { get; private set; }
        private MyUtils mu = new MyUtils();

        public EastTesterPage()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] EastTesterPage::EastTesterPage() {this.GetHashCode()}");
            XamlViewModel = Ioc.Default.GetService<EastTesterViewModel>();
        }
    }
}