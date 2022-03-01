using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Console_MVVMTesting.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Console_MVVMTesting.Services
{
    internal class PageService : IPageService
    {
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();
        private MyUtils mu = new MyUtils();

        public PageService()
        {
            mu.MyConsoleWriteLine($"PageService::PageService()");
            
            Configure<UserReceiverViewModel, UserReceiverPage>();
            Configure<UserSenderViewModel, UserSenderPage>();
            Configure<UserReceiver2ViewModel, UserReceiver2Page>();
            Configure<UserSender2ViewModel, UserSender2Page>();

            Configure<EastTesterViewModel, EastTesterPage>();
            Configure<LCSocketViewModel, LCSocketPage>();

        }

        public Type GetPageType(string key)
        {
            mu.MyConsoleWriteLine($"PageService::GetPageType()");
            Type pageType;
            lock (_pages)
            {
                if (!_pages.TryGetValue(key, out pageType))
                {
                    throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
                }
            }

            return pageType;
        }


        private void Configure<VM, V>()
        where VM : ObservableObject
        where V : MyPage
        {
            lock (_pages)
            {
                var key = typeof(VM).FullName;
                if (_pages.ContainsKey(key))
                {
                    throw new ArgumentException($"The key {key} is already configured in PageService");
                }

                var type = typeof(V);
                if (_pages.Any(p => p.Value == type))
                {
                    throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
                }

                _pages.Add(key, type);
            }
        }
    }
}