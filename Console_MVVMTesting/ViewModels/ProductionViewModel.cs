using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.ViewModels
{
    class ProductionViewModel : ObservableRecipient
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;

        private const string consoleColor = "LBLUE";


        #region XamlIsSocketInitialized1
        private bool isSocketInitialized1;
        public bool XamlIsSocketInitialized1
        {
            get { return isSocketInitialized1; }
            set { SetProperty(ref isSocketInitialized1, value); }
        }
        #endregion


        #region TestRegex
        private void TestRegex()
        {
            _log.Log(consoleColor, $"ProductionViewModel::TestRegex()");

            string heartbeat = "'0x40''0x0d''0x0a'";    // litosci!
            //_log.Log(consoleColor, $"{heartbeat}");

            string heartbeat1 = "@\r\n";
            _log.Log(consoleColor, $"{heartbeat1}");
            string heartbeat2 = "@\n\r";
            _log.Log(consoleColor, $"{heartbeat1}");

            byte[] myBytes = { 0x40, 0x0d, 0x0a, 0x40, 0x0d, 0x0a, 0x40, 0x0d, 0x0a, 0x40, 0x0d, 0x0a, 0x43, 0x44,
                0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x0d, 0x0a, 0x00 };
            char[] cArray = System.Text.Encoding.ASCII.GetString(myBytes).ToCharArray();

            string response = new string(cArray);
            MyUtils.DisplayStringInBytes(response);
            MyUtils.DisplayStringInBytes(response.Replace(heartbeat1, ""));
            MyUtils.DisplayStringInBytes(response.Replace(heartbeat2, ""));

        }
        #endregion TestRegex



        public MyUser GetCurrentUserAsync()
        {
            return new MyUser("from ProductionViewModel(): huj w dupe i kula wuep jebanemu putinowi");
        }


        private async Task OnStartButtonExecute()
        {
            _log.Log(consoleColor, $"ProductionViewModel::OnStartButtonExecute(): Start of method.");

            bool result;


            result = await this.InitLoadsTaskAsync();
            if (!result)
            {
                return;
            }

            result = await this.InitSocketsTaskAsync();
            if (!result)
            {
                return;
            }

            //result = await this.CheckBatteryStatusAndAlarmsTaskAsync();
            if (!result)
            {
                return;
            }

            result = await this.CheckPowerSupplyTaskAsync();
            if (!result)
            {
                return;
            }


            await Task.Delay(1000);

            result = this.ShutdownTaskAsync();
            if (!result)
            {
                return;
            }

            _log.Log(consoleColor, $"ProductionViewModel::OnStartButtonExecute(): End of method.");
            _log.Log(consoleColor, $"ProductionViewModel::OnStartButtonExecute(): Now press [Enter].");
        }



        private async Task<bool> InitLoadsTaskAsync()
        {
            _log.Log(consoleColor, "ProductionViewModel::InitLoadsTaskAsync(): Start of method");
            bool rs;

            // Run load-init in the EastTesterViewModule and request result
            EastTesterStateMessage etmsm = await _messenger.Send<EastTesterInitRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::InitLoadsTaskAsync(): etmsm.MyStateName: {etmsm.MyStateName}, etmsm.etStatus: {etmsm.etStatus}");

            if (etmsm.etStatus != ETStatus.Success)
            {
                _log.Log(consoleColor, $"ProductionViewModel::InitLoadsTaskAsync(): Load initializing failed with error.\n" + "Are loads connected and switched on ?");
                rs = false;
            }
            else
            {
                rs = true;
            }

            _log.Log(consoleColor, "ProductionViewModel::InitLoadsTaskAsync(): End of method");
            return rs;
        }


        private async Task<bool> InitSocketsTaskAsync()
        {
            _log.Log(consoleColor, "ProductionViewModel::InitSocketsTaskAsync(): Start of Task");
            bool rs = true;
            // Run init of sockets in the LCSocketViewModule and request result
            TRSocketStateMessage trmsm = await _messenger.Send<TRSocketInitRequestMessage>();
            foreach (KeyValuePair<IntPtr, Tuple<int, string>> entry in trmsm.SocketInitDict)
            {
                _log.Log(consoleColor, $"ProductionViewModel::InitSocketsTaskAsync(): socket {entry.Key}: {entry.Value}");
                if (entry.Value.Item1 != 0)
                {
                    rs = false;
                }
            }
            _log.Log(consoleColor, "ProductionViewModel::InitSocketsTaskAsync(): End of Task");
            return rs;
        }


        // wywala
        private async Task<bool> CheckBatteryStatusAndAlarmsTaskAsync()
        {
            _log.Log(consoleColor, "ProductionViewModel::CheckBatteryStatusAndAlarmsTaskAsync(): Start of Task");
            bool rs = false;
            TRSocketStateMessage trmsm = await _messenger.Send<CheckBatteryStatusAndAlarmsRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::CheckBatteryStatusAndAlarmsTaskAsync(): trmsm.MySocket.Keys.Count: {trmsm.BatteryStatusAndAlarmsDict.Keys.Count}");
            if (trmsm.BatteryStatusAndAlarmsDict.Keys.Count > 0)
            {
                //BatteryMode, BatteryStatus, SafetyAlertEnum, SafetyStatusEnum, PFAlertEnum, PFStatusEnum
                foreach (KeyValuePair<IntPtr, Tuple<UInt16, UInt16, UInt32, UInt32, UInt16, UInt32>> entry in trmsm.BatteryStatusAndAlarmsDict)
                {
                    //_log.Log(consoleColor, $"ProductionViewModel::CheckBatteryStatusAndAlarmsTaskAsync(): socket {entry.Key}: {entry.Value}");
                    _log.Log(consoleColor, $"ProductionViewModel::CheckBatteryStatusAndAlarmsTaskAsync(): " +
                        $"socket {entry.Key}: {entry.Value.Item1}, {entry.Value.Item2}, {entry.Value.Item3}, {entry.Value.Item4}, {entry.Value.Item5}, {entry.Value.Item6}");

                    if ((entry.Value.Item2 == 0) && (entry.Value.Item3 == 0) && (entry.Value.Item4 == 0) && (entry.Value.Item5 == 0) && (entry.Value.Item6 == 0))
                    {
                        rs = true;
                    }
                }
            }
            _log.Log(consoleColor, "ProductionViewModel::CheckBatteryStatusAndAlarmsTaskAsync(): End of Task");
            return rs;
        }


        // 2. Make sure that PS is alive and power source is AC - OM command
        private async Task<bool> CheckPowerSupplyTaskAsync()
        {
            _log.Log(consoleColor, "ProductionViewModel::CheckPowerSupplyTaskAsync(): Start of Task");

            TRSocketStateMessage trmsm = await _messenger.Send<TRSocketCheckPowerSupplyRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::CheckPowerSupplyTaskAsync(): trmsm.MySocket.Keys.Count: {trmsm.CheckPowerSupplyDict.Keys.Count}");
            if (trmsm.CheckPowerSupplyDict.Keys.Count > 0)
            {
                foreach (KeyValuePair<IntPtr, Tuple<string, double, int>> entry in trmsm.CheckPowerSupplyDict)
                {
                    _log.Log(consoleColor, $"ProductionViewModel::CheckPowerSupplyTaskAsync(): socket {entry.Key}: {entry.Value}");
                }
            }
            _log.Log(consoleColor, "ProductionViewModel::CheckPowerSupplyTaskAsync(): End of Task");
            return true;
        }


        private bool ShutdownTaskAsync()
        {
            // shutdown for loads
            EastTesterStateMessage etsm = _messenger.Send<EastTesterShutdownRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::IsShuttingDown.set: etsm.ETErrorNumber: {etsm.ETErrorNumber}");

            // shutdown for sockets
            TRSocketStateMessage trssm = _messenger.Send<TRShutdownRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::IsShuttingDown.set: trssm.TRErrorNumber: {trssm.TRErrorNumber}");

            return true;
        }



        #region Constructor
        public ProductionViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;

            //this.TestRegex();

            //Task.Delay(2000);
            //_messenger.Send(new InitETMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): InitETMessage()  ({GetHashCode():x8})");

            //_messenger.Send(new LCInitMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): LCInitMessage()  ({GetHashCode():x8})");

            //_messenger.Send(new LCCloseMessage());
            //_log.Log($"ProductionViewModel::ProductionViewModel(): LCCloseMessage()  ({GetHashCode():x8})");


            // request value from LCSocketViewModel
            //MyUser myUser = _messenger.Send<LoggedInUserRequestMessage>();
            //_log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): myUser._myName: {myUser.MyUserName}");


            // Register a message in some module
            _messenger.Register<LoggedInUserChangedMessage>(this, (r, m) =>
            {
                // Handle the message here, with r being the recipient and m being the
                // input messenger. Using the recipient passed as input makes it so that
                // the lambda expression doesn't capture "this", improving performance.

                _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel():  r.GetType(): { r.GetType()}; m.Value: {m.Value}");

            });




            // C:\Users\pak\Source\Repos\MVVM-Samples-master\samples\MvvmSampleUwp.sln
            //_messenger.Register<LCSocketStateMessage>(this, (r, m) =>
            //{
            //    if (m.LCStatus == LCSocketStatusEnum.Connected)
            //    {
            //        XamlIsSocketInitialized1 = true;
            //    }
            //    else
            //    {
            //        XamlIsSocketInitialized1 = false;
            //    }
            //    _log.Log($"ProductionViewModel::ProductionViewModel(): XamlIsSocketInitialized1 is {XamlIsSocketInitialized1}");

            //    m.Reply(m.LCStatus);
            //});


            Task MyTask = Task.Run(OnStartButtonExecute);
            MyTask.Wait();

            _log.Log(consoleColor, $"ProductionViewModel::ProductionViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }
        #endregion Constructor
    }
}