
namespace Console_MVVMTesting.Services
{
    public interface ILoggingService
    {
        void Log(string consoleColor, string message);
        void Log(string message);
    }
}
