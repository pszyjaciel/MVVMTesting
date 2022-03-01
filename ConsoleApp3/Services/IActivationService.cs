using System.Threading.Tasks;

namespace Console_MVVMTesting.Services
{
    public interface IActivationService
    {
        void ActivateAsync(object activationArgs);
    }
}
