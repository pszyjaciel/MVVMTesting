using System;
using System.Threading;
using System.Threading.Tasks;
using Console_MVVMTesting.Helpers;

namespace Console_MVVMTesting
{
    // FromEvent<>, based on http://stackoverflow.com/a/22798789/1768303
    public class TaskExt
    {
        private MyUtils mu;

        public TaskExt()
        {
            MyUtils mu = new MyUtils();
        }

        public async Task<TEventArgs> FromEvent<TEventHandler, TEventArgs>(
            Func<Action<TEventArgs>, Action, Action<Exception>, TEventHandler> getHandler,
            Action<TEventHandler> subscribe,
            Action<TEventHandler> unsubscribe,
            Action<Action<TEventArgs>, Action, Action<Exception>> initiate,
            CancellationToken token) where TEventHandler : class
        {
            mu.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] TaskExt::FromEvent()" +
                $"({typeof(SerialPortExtensions).GetHashCode():x8})");

            TaskCompletionSource<TEventArgs> tcs = new TaskCompletionSource<TEventArgs>();

            Action<TEventArgs> complete = (args) => tcs.TrySetResult(args);
            Action cancel = () => tcs.TrySetCanceled();
            Action<Exception> reject = (ex) => tcs.TrySetException(ex);

            TEventHandler handler = getHandler(complete, cancel, reject);

            subscribe(handler);
            try
            {
                using (token.Register(() => tcs.TrySetCanceled()))
                {
                    initiate(complete, cancel, reject);
                    return await tcs.Task;
                }
            }
            finally
            {
                unsubscribe(handler);
            }
        }
    }


}
