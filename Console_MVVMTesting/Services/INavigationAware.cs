using System.Threading.Tasks;

namespace Console_MVVMTesting.Services
{
    public interface INavigationAware
    {
        Task OnNavigatedTo(object parameter);

        void OnNavigatedFrom();
    }
}
