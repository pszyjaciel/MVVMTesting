using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
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
            _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo() - start of method");
            XamlSampleItems.Clear();

            // Replace this with your actual data
            System.Collections.Generic.IEnumerable<SampleOrder> myAllSampleOrders = await _sampleDataService.GetListDetailsDataAsync();
            _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo(): myAllSampleOrders.Count(): {myAllSampleOrders.Count()}");

            foreach (SampleOrder sampleOrder in myAllSampleOrders)
            {
                // pacz override ToString() w SampleOrder.cs
                //_log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo(): {sampleOrder.SymbolName} : {sampleOrder.Company} : {sampleOrder.OrderID} : {sampleOrder.OrderDate}");

                foreach (SampleOrderDetail sod in sampleOrder.Details)
                {
                    //_log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo(): {sod.ProductID} : {sod.ProductName} : {sod.Discount} : {sod.Quantity} of {sod.Total}");
                }
                XamlSampleItems.Add(sampleOrder);
            }

            System.Collections.Generic.IEnumerable<MySerialPort> myAllAvailableSerialPorts = await _sampleDataService.GetSerialPortsListDetailsDataAsync();
            _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo(): myAllAvailableSerialPorts.Count(): {myAllAvailableSerialPorts.Count()}");


            _log.Log(_consoleColor, $"ListLoadsViewModel::OnNavigatedTo(): - end of method");
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



        private void CallOnNavigatedTo()
        {
            _log.Log(_consoleColor, $"ListLoadsViewModel::CallOnNavigatedTo() - start of method");

            object myParam = null;
            Task myTask = Task.Run(async () => await OnNavigatedTo(myParam));
            myTask.Wait();

            _log.Log(_consoleColor, $"ListLoadsViewModel::CallOnNavigatedTo() - end of method");
        }


        #region Constructor
        public ListLoadsViewModel(ILoggingService loggingService, IMessenger messenger, ISampleDataService sampleDataService)
        {
            _log = loggingService;
            _log.Log(_consoleColor, $"ListLoadsViewModel::ListLoadsViewModel()");
            _messenger = messenger;
            _sampleDataService = sampleDataService;


            // stont moge wywolac co chcem.
            this.CallOnNavigatedTo();

        }
        #endregion Constructor


    }
}
