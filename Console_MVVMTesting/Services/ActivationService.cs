using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Views;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Services
{
    internal class ActivationService : IActivationService
    {
        private ShellPage _shellPage = null;
        private EastTesterPage _eastTesterPage = null;
        private LCSocketPage _lcSocketPage = null;

        private MyUtils mu;

        private UserReceiverPage _userReceiverPage = null;
        private UserSenderPage _userSenderPage = null;
        private UserReceiver2Page _userReceiver2Page = null;
        private UserSender2Page _userSender2Page = null;
        private ProductionPage _productionPage = null;


        private readonly IEnumerable<IActivationHandler> _activationHandlers;
        private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;

        public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers)
        {
            mu = new MyUtils();
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] ActivationService::ActivationService() {this.GetHashCode()}");

            _defaultHandler = defaultHandler;
            _activationHandlers = activationHandlers;
        }


        public void ActivateAsync(object activationArgs)
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] ActivationService::ActivateAsync() {this.GetHashCode()}");

            // Initialize services that you need before app activation
            // take into account that the splash screen is shown while this code runs.
            InitializeAsync();

            _shellPage = Ioc.Default.GetService<ShellPage>();
            Program.Content1 = _shellPage ?? new ShellPage();

            _eastTesterPage = Ioc.Default.GetService<EastTesterPage>();
            Program.Content2 = _eastTesterPage ?? new EastTesterPage();

            _lcSocketPage = Ioc.Default.GetService<LCSocketPage>();
            Program.Content3 = _lcSocketPage ?? new LCSocketPage();

            _userReceiverPage = Ioc.Default.GetService<UserReceiverPage>();
            Program.Content4 = _userReceiverPage ?? new UserReceiverPage();

            _userSenderPage = Ioc.Default.GetService<UserSenderPage>();
            Program.Content5 = _userSenderPage ?? new UserSenderPage();

            _userReceiver2Page = Ioc.Default.GetService<UserReceiver2Page>();
            Program.Content6 = _userReceiver2Page ?? new UserReceiver2Page();

            _userSender2Page = Ioc.Default.GetService<UserSender2Page>();
            Program.Content7 = _userSender2Page ?? new UserSender2Page();

            _productionPage = Ioc.Default.GetService<ProductionPage>();
            Program.Content8 = _productionPage ?? new ProductionPage();

            // Depending on activationArgs one of ActivationHandlers or DefaultActivationHandler will navigate to the first page
            HandleActivationAsync(activationArgs);

            // Ensure the current window is active
            Program.Activate();

            // Tasks after activation
            StartupAsync();
        }

        private void HandleActivationAsync(object activationArgs)
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] ActivationService::HandleActivationAsync() ({this.GetHashCode()})");

            IActivationHandler activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
            {
                activationHandler.HandleAsync(activationArgs);
            }

            if (_defaultHandler.CanHandle(activationArgs))
            {
                _defaultHandler.HandleAsync(activationArgs);
            }
        }


        private void InitializeAsync()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] ActivationService::InitializeAsync() ({this.GetHashCode()})");

            //await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
            //await Task.CompletedTask;
        }

        private void StartupAsync()
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] ActivationService::StartupAsync() ({this.GetHashCode()})");

            //await _themeSelectorService.SetRequestedThemeAsync();
            //await Task.CompletedTask;
        }

    }
}