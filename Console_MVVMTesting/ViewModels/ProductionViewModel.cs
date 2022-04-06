using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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



        //public MyUser GetCurrentUserAsync()
        //{
        //    return new MyUser("from ProductionViewModel(): huj w dupe i kula wuep jebanemu putinowi");
        //}


        private async Task OnStartButtonExecute()
        {
            _log.Log(consoleColor, $"ProductionViewModel::OnStartButtonExecute(): Start of method.");

            bool result = true;

            int numberOfSets = this.GetNumberOfSetsTask();
            if (numberOfSets < 0)
            {
                return;
            }

            //result = await this.InitLoadsTaskAsync();
            //if (!result)
            //{
            //    return;
            //}

            //result = await this.InitLCSocketsTaskAsync();
            //if (!result)
            //{
            //    return;
            //}

            result = await this.InitTRSocketsTaskAsync(numberOfSets);
            if (!result)
            {
                //return;
            }

            result = await this.TRSocketCheckBatteryStatusTaskAsync(numberOfSets);
            if (!result)
            {
                //return;
            }

            result = await this.TRSocketCheckBatteryAlarmsTaskAsync(numberOfSets);
            //if (!result) return;

            //result = await this.LCSocketCheckBatteryStatusTaskAsync();
            //if (!result)
            //{
            //    //return;
            //}

            //result = await this.LCSocketCheckBatteryAlarmsTaskAsync();
            //if (!result)
            //{
            //return;
            //}

            //result = await this.CheckPowerSupplyTaskAsync();
            //if (!result)
            //{
            //return;
            //}


            _log.Log(consoleColor, $"ProductionViewModel::OnStartButtonExecute(): In the delay task - before.");
            await Task.Delay(5000);
            _log.Log(consoleColor, $"ProductionViewModel::OnStartButtonExecute(): In the delay task - and after.");

            result = await this.ShutdownTaskAsync();
            if (!result)
            {
                return;
            }



            // init sockets one more time 
            //await Task.Delay(2000);

            //result = await this.InitSocketsTaskAsync();
            //if (!result)
            //{
            //    return;
            //}

            //result = await this.CheckPowerSupplyTaskAsync();
            //if (!result)
            //{
            //    return;
            //}

            //await Task.Delay(1000);

            //result = await this.ShutdownTaskAsync();
            //if (!result)
            //{
            //    return;
            //}


            _log.Log(consoleColor, $"ProductionViewModel::OnStartButtonExecute(): End of method.");
        }


        private int GetNumberOfSetsTask()
        {
            _log.Log(consoleColor, "ProductionViewModel::GetNumberOfSetsTask(): Start of Task");

            int numberOfSets = 2;
            _log.Log(consoleColor, $"ProductionViewModel::GetNumberOfSetsTask(): you wrote: {numberOfSets} sets.");
            _messenger.Send(new NumberOfSetsValueChangedMessage(numberOfSets));

            _log.Log(consoleColor, "ProductionViewModel::GetNumberOfSetsTask(): End of Task");
            return numberOfSets;
        }


        private async Task<bool> InitLoadsTaskAsync()
        {
            _log.Log(consoleColor, "ProductionViewModel::InitLoadsTaskAsync(): Start of Task");
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

            _log.Log(consoleColor, "ProductionViewModel::InitLoadsTaskAsync(): End of Task");
            return rs;
        }


        private async Task<bool> InitLCSocketsTaskAsync()
        {
            _log.Log(consoleColor, "ProductionViewModel::InitLCSocketsTaskAsync(): Start of Task");
            bool rs = true;

            // Run init of sockets in the LCSocketViewModule and request result
            LCSocketStateMessage trmsm = await _messenger.Send<LCSocketInitRequestMessage>();
            foreach (KeyValuePair<IntPtr, Tuple<int, string>> entry in trmsm.SocketInitDict)
            {
                _log.Log(consoleColor, $"ProductionViewModel::InitLCSocketsTaskAsync(): socket {entry.Key}: {entry.Value}");
                if (entry.Value.Item1 != 0) { rs = false; }
                else { rs = true; }
            }
            _log.Log(consoleColor, "ProductionViewModel::InitLCSocketsTaskAsync(): End of Task");
            return rs;
        }


        // initializes TR-Sockets
        private async Task<bool> InitTRSocketsTaskAsync(int numberOfSets)
        {
            _log.Log("ProductionViewModel::InitTRSocketsTaskAsync(): Start of method");
            bool rs = true;
            //XamlInitSocketsBackground = new SolidColorBrush(Colors.Yellow);

            // Run init of sockets in the TRSocketViewModule and request result
            TRSocketStateMessage trmsm = await _messenger.Send<TRSocketInitRequestMessage>();
            _log.Log($"ProductionViewModel::InitTRSocketsTaskAsync(): trmsm.SocketInitDict.Keys.Count: {trmsm.SocketInitDict.Keys.Count}");

            // check if number of initialized sets equals to numberOfSets
            if (trmsm.SocketInitDict.Keys.Count != numberOfSets)
            {
                _log.Log($"ProductionViewModel::InitTRSocketsTaskAsync(): The number of listening sockets does not equal " +
                    "to the number of sets you inserted.\n");
                //await _userNotificationService.MessageDialogAsync("Initialization of sockets", $"The number of listening sockets does not equal\n" +
                //    "to the number of sets you inserted.\n");
                //XamlInitSocketsBackground = new SolidColorBrush(Colors.LightPink);
                return false;
            }

            IntPtr socketHandle;
            int intValue;
            string strValue;

            foreach (KeyValuePair<IntPtr, Tuple<int, string>> entry in trmsm.SocketInitDict)
            {
                socketHandle = entry.Key;
                intValue = entry.Value.Item1;
                strValue = entry.Value.Item2;

                _log.Log($"ProductionViewModel::InitTRSocketsTaskAsync(): socket {socketHandle}: {entry.Value}");
                if (intValue != 0)
                {
                    //XamlInitSocketsBackground = new SolidColorBrush(Colors.LightPink);
                    rs = false;
                }
                else
                {
                    //XamlInitSocketsBackground = new SolidColorBrush(Colors.PaleGreen);
                    rs = true;
                }
            }

            if (!rs)
            {
                //await _userNotificationService.MessageDialogAsync("Initialization of sockets", $"Initializing failed.\n" +
                //    "Check if TR/LC is running.\n" + "Check if OM-terminal gets response.\n");
                _log.Log("ProductionViewModel::InitTRSocketsTaskAsync(): Initializing of TR-sockets failed.\n" +
                    "Check if TR is running.\n" + "Check if OM-terminal gets response.\n");
            }

            _log.Log("ProductionViewModel::InitTRSocketsTaskAsync(): End of method");
            return rs;
        }



        private async Task<bool> TRSocketCheckBatteryStatusTaskAsync(int numberOfSets)
        {
            _log.Log(consoleColor, "ProductionViewModel::TRSocketCheckBatteryStatusTaskAsync(): Start of Task");
            //XamlCheckBatteryStatusAndAlarmsBackground = new SolidColorBrush(Colors.Yellow);

            bool rs = false;
            TRSocketStateMessage trmsm = await _messenger.Send<CheckBatteryStatusRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::TRSocketCheckBatteryStatusTaskAsync(): trmsm.BatteryStatusDict.Keys.Count: {trmsm.BatteryStatusDict.Keys.Count}");

            // check if OK-status for all batteries equals to numberOfSets
            if (trmsm.BatteryStatusDict.Keys.Count != numberOfSets)
            {
                _log.Log(consoleColor, $"ProductionViewModel::TRSocketCheckBatteryStatusTaskAsync(): The OK-status of batteries does not equal to the number of sets you inserted.");
                //XamlCheckBatteryStatusAndAlarmsBackground = new SolidColorBrush(Colors.LightPink);
                //await _userNotificationService.MessageDialogAsync("Battery status", $"The OK-status of batteries does not equal to the number of sets you inserted.\n");
                return false;
            }

            if (trmsm.BatteryStatusDict.Keys.Count > 0)
            {
                IntPtr socketNumber;
                ushort batteryMode;
                ushort batteryStatus;
                string errorMessage;
                foreach (KeyValuePair<IntPtr, Tuple<UInt16, UInt16, string>> entry in trmsm.BatteryStatusDict)  //BatteryMode, BatteryStatus, BatteryErrorMessage
                {
                    socketNumber = entry.Key;
                    batteryMode = entry.Value.Item1;
                    batteryStatus = entry.Value.Item2;
                    errorMessage = entry.Value.Item3;

                    _log.Log(consoleColor, $"ProductionViewModel::TRSocketCheckBatteryStatusTaskAsync(): " +
                        $"socket {socketNumber}: {batteryMode:X4}, {batteryStatus:X4}, {errorMessage}");

                    if (batteryStatus == 0)
                    {
                        //XamlCheckBatteryStatusAndAlarmsBackground = new SolidColorBrush(Colors.PaleGreen);
                        rs = true;
                    }
                    else
                    {
                        //XamlCheckBatteryStatusAndAlarmsBackground = new SolidColorBrush(Colors.LightPink);
                        _log.Log(consoleColor, "ProductionViewModel::TRSocketCheckBatteryStatusTaskAsync(): here we have to parse the error");
                        //this.ParseBatteryStatus();
                        //_userNotificationService.MessageDialogAsync("Battery status", $"Battery status failed with error:\n{errorMessage}\n" +
                        //    "Check if battery is connected.\nCheck if OM gets response from battery.\n");
                        rs = false;
                    }
                }
            }
            _log.Log("ProductionViewModel::TRSocketCheckBatteryStatusTaskAsync(): End of Task");
            return rs;
        }

        private async Task<bool> TRSocketCheckBatteryAlarmsTaskAsync(int numberOfSets)
        {
            _log.Log(consoleColor, "ProductionViewModel::TRSocketCheckBatteryAlarmsTaskAsync(): Start of Task");
            bool rs = false;
            TRSocketStateMessage trmsm = await _messenger.Send<TRSocketCheckBatteryAlarmsRequestMessage>();

            // check if no alarms has been notified for all numberOfSets
            if (trmsm.BatteryAlarmsDict.Keys.Count != numberOfSets)
            {
                _log.Log(consoleColor, $"ProductionViewModel::TRSocketCheckBatteryAlarmsTaskAsync(): Could not determine all alarms for sets you inserted");
                // _userNotificationService.MessageDialogAsync("Battery alarms", $"Could not determine all alarms for sets you inserted.\n");
                //XamlCheckBatteryStatusAndAlarmsBackground = new SolidColorBrush(Colors.LightPink);
                return false;
            }

            IntPtr mySocketHandle;
            UInt32 SafetyAlertEnum;
            UInt32 SafetyStatusEnum;
            UInt16 PFAlertEnum;
            UInt32 PFStatusEnum;
            string BatteryError;

            _log.Log(consoleColor, $"ProductionViewModel::TRSocketCheckBatteryAlarmsTaskAsync(): trmsm.MySocket.Keys.Count: {trmsm.BatteryAlarmsDict.Keys.Count}");
            if (trmsm.BatteryAlarmsDict.Keys.Count > 0)
            {
                //SafetyAlertEnum, SafetyStatusEnum, PFAlertEnum, PFStatusEnum
                foreach (KeyValuePair<IntPtr, Tuple<UInt32, UInt32, UInt16, UInt32, string>> entry in trmsm.BatteryAlarmsDict)
                {
                    mySocketHandle = entry.Key;
                    SafetyAlertEnum = entry.Value.Item1;
                    SafetyStatusEnum = entry.Value.Item2;
                    PFAlertEnum = entry.Value.Item3;
                    PFStatusEnum = entry.Value.Item4;
                    BatteryError = entry.Value.Item5;

                    _log.Log(consoleColor, $"ProductionViewModel::TRSocketCheckBatteryAlarmsTaskAsync(): " +
                        $"socket {mySocketHandle}: {SafetyAlertEnum}, {SafetyStatusEnum}, {PFAlertEnum}, {PFStatusEnum}, {BatteryError}");

                    if ((SafetyAlertEnum == 0) && (SafetyStatusEnum == 0) && (PFAlertEnum == 0) && (PFStatusEnum == 0) && (BatteryError.Equals("")))
                    {
                        rs = true;
                    }
                    else
                    {
                        //this.ParseBatteryAlarms();
                    }
                }
            }
            _log.Log(consoleColor, "ProductionViewModel::TRSocketCheckBatteryAlarmsTaskAsync(): End of Task");
            return rs;
        }


        // 2. Make sure that PS is alive and power source is AC - OM command
        private async Task<bool> TRSocketCheckPowerSupplyTaskAsync()
        {
            _log.Log("ProductionViewModel::TRSocketCheckPowerSupplyTaskAsync(): Start of Task");
            //XamlCheckPowerSupplyBackground = new SolidColorBrush(Colors.Yellow);

            TRSocketStateMessage trmsm = await _messenger.Send<TRSocketCheckPowerSupplyRequestMessage>();
            _log.Log($"ProductionViewModel::TRSocketCheckPowerSupplyTaskAsync(): trmsm.MySocket.Keys.Count: {trmsm.CheckPowerSupplyDict.Keys.Count}");
            if (trmsm.CheckPowerSupplyDict.Keys.Count > 0)
            {
                foreach (KeyValuePair<IntPtr, Tuple<string, double, int>> entry in trmsm.CheckPowerSupplyDict)
                {
                    _log.Log($"ProductionViewModel::TRSocketCheckPowerSupplyTaskAsync(): socket {entry.Key}: {entry.Value}");
                }
            }
            //XamlCheckPowerSupplyBackground = new SolidColorBrush(Colors.PaleGreen);
            _log.Log("ProductionViewModel::TRSocketCheckPowerSupplyTaskAsync(): End of Task");
            return true;
        }




        private async Task<bool> LCSocketCheckBatteryStatusTaskAsync()
        {
            _log.Log(consoleColor, "ProductionViewModel::LCSocketCheckBatteryStatusTaskAsync(): Start of Task");
            bool rs = false;
            LCSocketStateMessage lcssm = await _messenger.Send<LCSocketCheckBatteryStatusRequestMessage>();
            //_log.Log(consoleColor, $"ProductionViewModel::CheckBatteryStatusTaskAsync(): trmsm.MySocket.Keys.Count: {lcssm.BatteryStatusDict.Keys.Count}");

            IntPtr socketHandle;
            UInt16 batteryMode;
            UInt16 batteryStatus;
            if (lcssm.BatteryStatusDict.Keys.Count > 0)
            {
                //BatteryMode, BatteryStatus
                foreach (KeyValuePair<IntPtr, Tuple<UInt16, UInt16>> entry in lcssm.BatteryStatusDict)
                {
                    socketHandle = entry.Key;
                    batteryMode = entry.Value.Item1;
                    batteryStatus = entry.Value.Item2;

                    _log.Log(consoleColor, $"ProductionViewModel::LCSocketCheckBatteryStatusTaskAsync(): " +
                        $"socket {socketHandle}: {batteryMode}, {batteryStatus}");

                    if (batteryStatus == 0)
                    {
                        rs = true;
                    }
                    else
                    {
                        //this.ParseBatteryStatus();
                    }
                }
            }
            _log.Log(consoleColor, "ProductionViewModel::LCSocketCheckBatteryStatusTaskAsync(): End of Task");
            return false;
        }





        private async Task<bool> ShutdownTaskAsync()
        {
            // shutdown for loads
            EastTesterStateMessage etsm = _messenger.Send<EastTesterShutdownRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::IsShuttingDown.set: etsm.ETErrorNumber: {etsm.ETErrorNumber}");

            // shutdown for sockets
            TRSocketStateMessage trssm = await _messenger.Send<TRShutdownRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::IsShuttingDown.set: trssm.TRErrorNumber: {trssm.TRErrorNumber}");

            LCSocketStateMessage lcssm = await _messenger.Send<LCShutdownRequestMessage>();
            _log.Log(consoleColor, $"ProductionViewModel::IsShuttingDown.set: trssm.TRErrorNumber: {lcssm.LCErrorNumber}");


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