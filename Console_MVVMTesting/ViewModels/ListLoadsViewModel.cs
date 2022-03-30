using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace Console_MVVMTesting.ViewModels
{
    public class ListLoadsViewModel : ObservableRecipient, INavigationAware
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;
        private readonly ISampleDataService _sampleDataService;

        private const string _consoleColor = "LRED";


        private SampleOrder _selected;
        public SampleOrder XamlSelected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        private MySerialPort _COMselected;
        public MySerialPort XamlCOMSelected
        {
            get { return _COMselected; }
            set { SetProperty(ref _COMselected, value); }
        }


        public ObservableCollection<SampleOrder> XamlSampleItems { get; private set; } = new ObservableCollection<SampleOrder>();
        public ObservableCollection<MySerialPort> XamlCOMItems { get; private set; } = new ObservableCollection<MySerialPort>();


        public async Task OnNavigatedTo(object parameter)
        {
            _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo()");
            XamlSampleItems.Clear();

            // Replace this with your actual data
            System.Collections.Generic.IEnumerable<SampleOrder> data = await _sampleDataService.GetListDetailsDataAsync();
            _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo(): data.Count(): {data.Count()}");

            foreach (SampleOrder item in data)
            {
                _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo(): item: {item}");
                XamlSampleItems.Add(item);
            }
        }

        public void OnNavigatedFrom()
        {
            _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedFrom()");
        }



        public void EnsureItemSelected()
        {
            _log.Log(_consoleColor, $"ListLoadsViewModel::EnsureItemSelected()");
            if (XamlSelected == null)
            {
                XamlSelected = XamlSampleItems.First();
            }
        }


        #region Constructor
        public ListLoadsViewModel(ILoggingService loggingService, IMessenger messenger, ISampleDataService sampleDataService)
        {
            _log = loggingService;
            _log.Log(_consoleColor, $"ListLoadsViewModel::ListLoadsViewModel()");
            _messenger = messenger;
            _sampleDataService = sampleDataService;
        }
        #endregion Constructor


    }
}
