using Console_MVVMTesting.Helpers;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Console_MVVMTesting
{
    public class MySerialPort
    {
        private Semaphore _serialSemaphore;

        public MySerialPort()
        {
            _serialSemaphore = new Semaphore(1, 1, "MySemaphore");
        }



        private async Task<bool> myBackgroudTaskAsync1()
        {
            MyUtils.MyConsoleWriteLine($"Program::myBackgroudTaskAsync1(): Start of method");
            List<SerialPort> myListOfSerialPorts = RunSerialPortSync();
            MyUtils.MyConsoleWriteLine($"Program::myBackgroudTaskAsync1(): {myListOfSerialPorts.Count}");

            if (myListOfSerialPorts.Count > 0)
                return true;

            await Task.CompletedTask;
            MyUtils.MyConsoleWriteLine($"Program::myBackgroudTaskAsync1(): End of method");
            return false;
        }

        //private static async Task<bool> myBackgroudTaskAsync2()
        //{
        //    MyUtils.MyConsoleWriteLine($"Program::myBackgroudTaskAsync2(): Start of method");
        //    await Task.Delay(5000);
        //    await Task.CompletedTask;
        //    MyUtils.MyConsoleWriteLine($"Program::myBackgroudTaskAsync2(): End of method");
        //    return false;
        //}

        //private async static Task MyCallbackTaskAsync()
        //{
        //    MyUtils.MyConsoleWriteLine($"Program::MyCallbackTaskAsync(): Start of method");
        //    await Task.Delay(6000);
        //    await Task.CompletedTask;
        //    MyUtils.MyConsoleWriteLine($"Program::MyCallbackTaskAsync(): End of method");
        //}




        private List<SerialPort> RunSerialPortSync()
        {
            List<SerialPort> myListOfSerialPorts = RetrieveSerialPortParallel();
            MyUtils.MyConsoleWriteLine($"Program::RunSerialPortAsync(): {myListOfSerialPorts.Count}");

            foreach (SerialPort mySerialPortName in myListOfSerialPorts)
            {
                MyUtils.MyConsoleWriteLine($"Program::RunSerialPortAsync(): Found: {mySerialPortName.PortName}");
            }

            CloseSerialPortParallelSync(myListOfSerialPorts);
            return myListOfSerialPorts;
        }


        private void CloseSerialPortParallelSync(List<SerialPort> myListOfSerialPorts)
        {
            foreach (SerialPort sp in myListOfSerialPorts)
            {
                try
                {
                    MyUtils.MyConsoleWriteLine($"Program::CloseSerialPortParallelSync(): {sp.PortName} is going close.");
                    sp.Close();
                }
                catch (Exception ex)
                {
                    MyUtils.MyConsoleWriteLine($"Program::CloseSerialPortParallelSync(): {ex.Message}");
                }
            }
        }




        private List<SerialPort> FindAndOpenSerialPorts()
        {
            MyUtils.MyConsoleWriteLine($"Program::FindAndOpenSerialPorts(): start of method");
            //await Task.Yield(); // Make us async right away

            List<SerialPort> mySerialPortList = new List<SerialPort>();

            string[] SerialPortNameArray = SerialPort.GetPortNames(); //Get available serial port
            foreach (string mySerialPortName in SerialPortNameArray)
            {
                SerialPort mySerialPort = new SerialPort(mySerialPortName, 115200, Parity.None, 8, StopBits.One);
                mySerialPort.ReadTimeout = 2500;
                mySerialPort.WriteTimeout = 1500;
                mySerialPortList.Add(mySerialPort);

                if (!mySerialPort.IsOpen)
                {
                    try
                    {
                        mySerialPort.Open();
                    }
                    catch (Exception ex)
                    {
                        MyUtils.MyConsoleWriteLine($"Program::FindAndOpenSerialPorts(): {ex.Message}");
                    }
                }
                MyUtils.MyConsoleWriteLine($"Program::FindAndOpenSerialPorts(): is {mySerialPort.PortName} open: {mySerialPort.IsOpen}");
            }
            return mySerialPortList;
        }



        // async doesn't work well with ForEach: https://stackoverflow.com/a/23139769/7036047
        // The whole idea behind Parallel.ForEach() is that you have a set of threads and each thread processes part of the collection.
        // This doesn't work with async-await, where you want to release the thread for the duration of the async call.
        private List<SerialPort> RetrieveSerialPortParallel()
        {
            MyUtils.MyConsoleWriteLine($"Program::RetrieveSerialPortParallel(): Start of method");
            //await Task.Yield();

            List<SerialPort> myListOfOpenedSerialPorts = FindAndOpenSerialPorts();
            foreach (SerialPort mySerialPortName in myListOfOpenedSerialPorts)
            {
                MyUtils.MyConsoleWriteLine($"Program::RetrieveSerialPortParallel(): mySerialPortName: {mySerialPortName.PortName}");
            }

            List<SerialPort> myInitializedListOfSerialPorts = new List<SerialPort>();
            bool initResult;
            Parallel.ForEach(myListOfOpenedSerialPorts, (mySerialPort) =>
            {
                initResult = InitializeSerialPort(mySerialPort);
                MyUtils.MyConsoleWriteLine($"Program::RetrieveSerialPortParallel(): initResult: {initResult}");
                if (initResult)
                {
                    myInitializedListOfSerialPorts.Add(mySerialPort);
                }
            });

            MyUtils.MyConsoleWriteLine($"Program::RetrieveSerialPortParallel(): myInitializedListOfSerialPorts.Count: {myInitializedListOfSerialPorts.Count}");
            MyUtils.MyConsoleWriteLine($"Program::RetrieveSerialPortParallel(): End of method");
            return myInitializedListOfSerialPorts;
        }

        private bool InitializeSerialPort(SerialPort mySerialPort)
        {
            MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): Start of method");


            Dictionary<string, string> ETInitCommandDict = new Dictionary<string, string>();

            string myCommand = "*IDN?" + '\n';
            string response;
            try
            {
                response = SerialPortExtensions.SendCommand(mySerialPort, myCommand);
                MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): response: {response}");
            }
            catch (TimeoutException te)
            {
                MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): {te.Message}");
                return false;
            }

            if (!response.Contains("ET5410"))
            {
                MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): Wrong device. Not ET5410.");
                return false;
            }

            /* CP mode */
            ETInitCommandDict.Add("CH:MODE", "CP");
            ETInitCommandDict.Add("CURR:IMAX", "5.0");
            ETInitCommandDict.Add("POWE:PMAX", "300");
            ETInitCommandDict.Add("POWE:CP", "100");

            // async write and await write result
            List<SerialPort> mySerialPortList = new List<SerialPort>();
            foreach (KeyValuePair<string, string> kvp in ETInitCommandDict)
            {
                myCommand = kvp.Key + " " + kvp.Value + '\n';   // cmd:value
                try
                {
                    response = SerialPortExtensions.SendCommand(mySerialPort, myCommand);
                }
                catch (TimeoutException te)
                {
                    MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): {te.Message}");
                    return false;
                }

                MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): response: {response}");
                if (response.Contains("err"))
                {
                    MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): response contains 'err'.");
                    return false;
                }
            }

            MyUtils.MyConsoleWriteLine($"Program::InitializeSerialPort(): End of method");
            return true;
        }


        private async Task<bool> InitializeETAsync(SerialPort mySerialPort)
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                 $"Program::InitializeETAsync(): mySerialPort.PortName: {mySerialPort.PortName}" +
                 $"({typeof(SerialPortExtensions).GetHashCode():x8})");


            await Task.Yield();

            if (mySerialPort == null)
            {
                MyUtils.MyConsoleWriteLine("Program::InitializeETAsync(): mySerialPort == null");
                return false;
            }

            Dictionary<string, string> ETInitCommandDict = new Dictionary<string, string>();

            // Start the transaction by waiting until the mutex is granted
            bool hasHandle = _serialSemaphore.WaitOne(1000);
            if (!hasHandle)
            {
                return false;
            }
            MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): Semaphore aquired on thread {Thread.CurrentThread.ManagedThreadId}");

            string myCommand = "*IDN?" + '\n';
            string response = null;
            try
            {
                //await SerialPortExtensions.ReadLineAsync(mySerialPort);
                response = await SerialPortExtensions.SendCommandAsync(mySerialPort, myCommand);
                MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): {response}");
            }
            catch (AggregateException ex)
            {
                MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): AggregateException: {ex.InnerException.Message}");
            }

            catch (OperationCanceledException oce)
            {
                MyUtils.MyConsoleWriteLine($"SerialPortExtensions::InitializeETAsync(): OperationCanceledException: {oce.Message}");
                return false;
            }

            catch (TimeoutException te)
            {
                MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): TimeoutException: {te.Message}");

                _serialSemaphore.Release();
                MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): In catch: Semaphore released on thread {Thread.CurrentThread.ManagedThreadId}");

                // maybe close the serial port
                return false;
            }

            if (!response.Contains("ET5410"))
            {
                MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): Wrong device. Not ET5410.");

                _serialSemaphore.Release();
                MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): Semaphore released on thread {Thread.CurrentThread.ManagedThreadId}");

                mySerialPort.Close();
                return false;
            }

            // ...


            _serialSemaphore.Release();
            MyUtils.MyConsoleWriteLine($"Program::InitializeETAsync(): Semaphore released on thread {Thread.CurrentThread.ManagedThreadId}");
            return true;
        }



        private async Task<List<SerialPort>> OpenCOMPortsAsync()
        {
            MyUtils.MyConsoleWriteLine($"Program::OpenCOMPortsAsync(): start of method");
            await Task.Yield(); // Make us async right away

            List<SerialPort> myAvailableSerialPortList = new List<SerialPort>();

            // lista portuf jako lista stringuf
            string[] SerialPortNameArray = SerialPort.GetPortNames();
            foreach (string mySerialPortName in SerialPortNameArray)
            {
                SerialPort mySerialPort = new SerialPort(mySerialPortName, 115200, Parity.None, 8, StopBits.One);
                mySerialPort.ReadTimeout = 3000;
                mySerialPort.WriteTimeout = 3000;

                myAvailableSerialPortList.Add(mySerialPort);

                if (!mySerialPort.IsOpen)
                {
                    try
                    {
                        mySerialPort.Open();
                    }
                    catch (Exception ex)
                    {
                        MyUtils.MyConsoleWriteLine($"Program::OpenCOMPortsAsync(): {ex.Message}");
                    }
                }
                //MyUtils.MyConsoleWriteLine($"Program::OpenCOMPortsAsync(): is {mySerialPort.PortName} open: {mySerialPort.IsOpen}");
            }

            //MyUtils.MyConsoleWriteLine($"Program::OpenCOMPortsAsync(): before Init(): {myAvailableSerialPortList.Count}");

            List<SerialPort> myInitializedSerialPortList = new List<SerialPort>();
            bool initResult;
            foreach (SerialPort mySerialPort in myAvailableSerialPortList)
            {
                initResult = await InitializeETAsync(mySerialPort);
                if (initResult)
                {
                    myInitializedSerialPortList.Add(mySerialPort);
                }
            }

            //MyUtils.MyConsoleWriteLine($"Program::OpenCOMPortsAsync(): after Init(): {myInitializedSerialPortList.Count}");

            MyUtils.MyConsoleWriteLine($"Program::OpenCOMPortsAsync(): end of method");
            return myInitializedSerialPortList;
        }



        internal void RunSerialPort()
        {
            MyUtils.MyConsoleWriteLine($"Program::RunSerialPort(): start of method");

            List<SerialPort> myListOfSerialPorts = new List<SerialPort>();

            //Task MyTask = Task.Run(async () => { myListOfSerialPorts = await OpenCOMPortsAsync(); });

            try
            {
                //Task myTask = OpenCOMPortsAsync();
                Task myTask = Task.Run(async () => { myListOfSerialPorts = await OpenCOMPortsAsync(); });
                myTask.Wait();
                MyUtils.MyConsoleWriteLine($"Program::RunSerialPort(): myListOfSerialPorts: {myListOfSerialPorts.Count}");
            }
            catch (AggregateException aex)
            {
                MyUtils.MyConsoleWriteLine($"Program::RunSerialPort(): AggregateException: {aex.Message}");
            }

            MyUtils.MyConsoleWriteLine($"Program::RunSerialPort(): end of method");
        }



    }
}
