using System.Collections.Generic;
using System.Threading.Tasks;
using Console_MVVMTesting.Models;


namespace Console_MVVMTesting.Services
{
    // Remove this class once your pages/features are using your data.
    public interface ISampleDataService
    {
        Task<IEnumerable<SampleOrder>> GetContentGridDataAsync();

        Task<IEnumerable<SampleOrder>> GetGridDataAsync();

        Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();

        Task<IEnumerable<MySerialPort>> GetSerialPortsListDetailsDataAsync();
    }
}
