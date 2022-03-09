using Console_MVVMTesting.Helpers;
using System;

namespace Console_MVVMTesting.Services
{
    public class LoggingService : ILoggingService
    {
        protected static int origRow;
        protected static int origCol;

        protected static void WriteAt(string s, int x, int y)
        {
            try
            {
                Console.SetCursorPosition(origCol + x, origRow + y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }

        public void Log(string consoleColor, string message)
        {
            MyUtils.MyConsoleWriteLine(consoleColor, $"[{DateTime.Now.ToString("HH:mm:ss.ff")}] {message} ");
        }

        public void Log(string message)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] {message} ");
        }
    }
}
