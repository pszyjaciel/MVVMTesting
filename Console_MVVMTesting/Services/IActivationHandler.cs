using System.Threading.Tasks;

namespace Console_MVVMTesting.Services
{
    internal interface IActivationHandler
    {
        bool CanHandle(object activationArgs);
        void HandleAsync(object activationArgs);
    }
}