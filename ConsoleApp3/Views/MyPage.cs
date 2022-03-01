﻿using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;

namespace Console_MVVMTesting.Views
{
    public class MyPage
    {
        
        private MyUtils mu;
        public MyPage()
        {
            mu = new MyUtils();

            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"MyPage::MyPage() ({this.GetHashCode():x8})");

        }
    }
}