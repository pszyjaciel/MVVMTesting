using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;


namespace Console_MVVMTesting.Views
{
    class ProductionPage : MyPage
    {
        public ProductionViewModel XamlProductionViewModel { get; private set; }
        

        public ProductionPage()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"ProductionPage::ProductionPage() " +
               $"({this.GetHashCode():x8})");

            XamlProductionViewModel = Ioc.Default.GetService<ProductionViewModel>();
        }
    }
}
