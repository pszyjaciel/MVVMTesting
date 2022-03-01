using Console_MVVMTesting.Helpers;
using System;
using System.Threading.Tasks;


namespace Console_MVVMTesting.Services
{
    public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
    {
        private MyUtils mu;

        public DefaultActivationHandler()
        {
            mu = new MyUtils();
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] DefaultActivationHandler::DefaultActivationHandler() {this.GetHashCode()}");
        }

        protected override void HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] DefaultActivationHandler::HandleInternalAsync() {this.GetHashCode()}");

            //await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            mu.MyConsoleWriteLine($"DefaultActivationHandler::CanHandleInternal()");

            // None of the ActivationHandlers has handled the app activation
            return true;
        }
    }
}
