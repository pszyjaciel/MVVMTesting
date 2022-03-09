using Console_MVVMTesting.Helpers;
using System;
using System.Threading.Tasks;


// For more information on understanding activation flow see
// https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/WinUI/activation.md
//
// Extend this class to implement new ActivationHandlers

namespace Console_MVVMTesting.Services
{
    public abstract class ActivationHandler<T> : IActivationHandler where T : class
    {
        protected ActivationHandler()
        {
        }

        // Override this method to add the activation logic in your activation handler
        protected abstract void HandleInternalAsync(T args);

        public void HandleAsync(object args)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"{$"ActivationHandler::HandleAsync()"} ({this.GetHashCode():x8})");

            HandleInternalAsync(args as T);
        }

        public bool CanHandle(object args)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"{$"ActivationHandler::CanHandle()"} ({this.GetHashCode():x8})");

            // CanHandle checks the args is of type you have configured
            return args is T && CanHandleInternal(args as T);
        }

        // You can override this method to add extra validation on activation args
        // to determine if your ActivationHandler should handle this activation args
        protected virtual bool CanHandleInternal(T args)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"{$"ActivationHandler::CanHandleInternal()"} ({this.GetHashCode():x8})");

            return true;
        }

    }
}