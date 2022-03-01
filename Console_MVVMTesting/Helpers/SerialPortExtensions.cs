using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;


namespace Console_MVVMTesting.Helpers
{
    public static class SerialPortExtensions
    {

        #region Async methods

        // https://git.am-wd.de/AM.WD/Modbus

        public static async Task<string> ReadLineAsync(this SerialPort _serialPort)
        {
            //Debug.WriteLine("SerialPortExtensions::ReadLineAsync(): start of method");

            byte[] buffer = new byte[1];
            string responseBuffer = string.Empty;
            char myChar;

            CancellationTokenSource cts = new CancellationTokenSource(_serialPort.ReadTimeout);
            CancellationToken mySerialPortCancellationToken = new CancellationToken();
            mySerialPortCancellationToken.Register(() => cts.Cancel());

            // musi ma byc
            CancellationTokenRegistration ctr = default(CancellationTokenRegistration);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // The async stream implementation on windows is a bit broken.
                // this kicks it back to us.
                // DiscardInBuffer definitely forces ReadAsync to return.
                ctr = cts.Token.Register(() => _serialPort.DiscardInBuffer());
            }

            while (true)
            {
                try
                {
                    await _serialPort.BaseStream.ReadAsync(buffer, 0, 1, cts.Token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] {$"SerialPortExtensions::ReadLineAsync(): Exception: {ex.Message}"} " +
                        $"({typeof(SerialPortExtensions).GetHashCode():x8})");

                    break;
                }
                finally
                {
                    ctr.Dispose();
                }
                myChar = (char)buffer[0];       // bo jeden znak
                if (myChar != '\n')
                {
                    responseBuffer += myChar;
                }
                else
                {
                    break;
                }
            }   // end of while loop

            cts.Dispose();
            //Debug.WriteLine("SerialPortExtensions::ReadLineAsync(): end of method");
            return responseBuffer;
        }


        /// <summary>
        /// Writes a line to the serial port asynchronously.
        /// </summary>
        /// <param name="_serialPort">The serial port.</param>
        /// <param name="toWrite">String to write.</param>
        public static async Task WriteLineAsync(this SerialPort _serialPort, string toWrite)
        {
            //Debug.WriteLine("SerialPortExtensions::WriteLineAsync(): start of method");

            byte[] writeToByte;

            if (_serialPort == null)
                return;

            CancellationTokenSource cts = new CancellationTokenSource(_serialPort.WriteTimeout);
            CancellationToken mySerialPortCancellationToken = new CancellationToken();
            mySerialPortCancellationToken.Register(() => cts.Cancel());

            // musi ma byc
            CancellationTokenRegistration ctr = default(CancellationTokenRegistration);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // DiscardOutBuffer definitely forces WriteAsync to return.
                ctr = cts.Token.Register(() => _serialPort.DiscardOutBuffer());
            }

            try
            {
                writeToByte = _serialPort.Encoding.GetBytes(toWrite);

                await _serialPort.BaseStream.WriteAsync(writeToByte, 0, writeToByte.Length, cts.Token).ConfigureAwait(false);
                await _serialPort.BaseStream.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SerialPortExtensions::WriteLineAsync(): {ex.Message}");
            }
            finally
            {
                ctr.Dispose();
            }
            cts.Dispose();
            //Debug.WriteLine("SerialPortExtensions::WriteLineAsync(): end of method");
        }

        // https://stackoverflow.com/questions/23230375/sample-serial-port-comms-code-using-async-api-in-net-4-5

        /// <summary>
        /// Sends a command to the connected serial port and returns its response in string.
        /// </summary>
        /// <param name="command">The command to send to through the port.</param>
        /// <returns>A response string of the given command.</returns>
        public static async Task<string> SendCommandAsync(this SerialPort _serialPort, string command)
        {
            //Debug.WriteLine("SerialPortExtensions::SendCommandAsync(): start of method");

            // Write
            await _serialPort.WriteLineAsync(command);

            // Read
            string readLineTask = await _serialPort.ReadLineAsync();

            //string myResult = await readLineTask.ConfigureAwait(false);
            //Debug.WriteLine("SerialPortExtensions::SendCommandAsync(): end of method");
            return readLineTask;
        }

        /// <summary>
        /// Sends a list of commands to the connected serial port and returns its responses.
        /// </summary>
        /// <param name="commands">The commands. String parameter containing commands separated by a delimiter</param>
        /// <param name="delimiter">The delimiter to use for splitting the input string. Default: '&'</param>
        /// <param name="delay">The delay between sending commands in Milliseconds. Default: 0</param>
        /// <returns></returns>
        /// 
        // pacz nazwa metody i literka 's' w commands
        public static async Task<List<string>> SendCommandsAsync(this SerialPort _serialPort, string commands, char delimiter = '&', int delay = 0)
        {
            List<string> responses = new List<string>();
            foreach (string command in commands.Split(delimiter))
            {
                await Task.Delay(delay).ConfigureAwait(false);
                responses.Add(await _serialPort.SendCommandAsync(command).ConfigureAwait(false));
            }
            return responses;
        }


        /// <summary>
        /// Throws a TimeoutException when the timeout task completed before the input task
        /// is completed.
        /// </summary>
        /// <param name="_task">The primary task.</param>
        /// <param name="timeoutInMilliseconds">The timeout time in milliseconds.</param>
        private static async Task CheckTimeoutAsync(Task _task, int timeoutInMilliseconds)
        {
            Task timeoutTask = Task.Delay(timeoutInMilliseconds);
            Task completedReadTask = await Task.WhenAny(_task, timeoutTask).ConfigureAwait(false);
            if (completedReadTask == timeoutTask)
            {
                throw new TimeoutException("Task took longer than expected.\n");
            }
        }
        #endregion Async methods


        #region Sync Methods
        public static string ReadFromBuffer2(this SerialPort _serialPort)
        {
            //Debug.WriteLine("SerialPortExtensions::ReadFromBuffer(): start of method");
            //Debug.WriteLine($"SerialPortExtensions::ReadFromBuffer(): _serialPort.BytesToRead: {_serialPort.BytesToRead}");

            int buffer = 0;
            string responseBuffer = string.Empty;
            char myChar;

            while (true)
            {
                try
                {
                    buffer = _serialPort.ReadByte();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SerialPortExtensions::ReadFromBuffer(): {ex.Message}");
                    break;
                }

                myChar = (char)buffer;       // bo jeden znak
                if (myChar != '\n')
                {
                    responseBuffer += myChar;
                }
                else
                {
                    break;
                }
            }
            //Debug.WriteLine("SerialPortExtensions::ReadFromBuffer(): end of method");
            return responseBuffer;
        }

        public static void WriteToBuffer(this SerialPort _serialPort, string toWrite)
        {
            byte[] writeToByte;

            writeToByte = _serialPort.Encoding.GetBytes(toWrite);
            _serialPort.Write(writeToByte, 0, writeToByte.Length);
        }



        public static string ReadFromBuffer(this SerialPort _serialPort)
        {
            //Debug.WriteLine("SerialPortExtensions::ReadFromBuffer(): start of method");
            //Debug.WriteLine($"SerialPortExtensions::ReadFromBuffer(): _serialPort.BytesToRead: {_serialPort.BytesToRead}");

            int buffer = 0;
            string responseBuffer = string.Empty;
            char myChar;

            try
            {
                responseBuffer = _serialPort.ReadLine();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SerialPortExtensions::ReadFromBuffer(): {ex.Message}");
            }

            myChar = (char)buffer;       // bo jeden znak
            if (myChar != '\n')
            {
                responseBuffer += myChar;
            }
            //Debug.WriteLine("SerialPortExtensions::ReadFromBuffer(): end of method");
            return responseBuffer;
        }


        public static string SendCommand(this SerialPort _serialPort, string command)
        {
            // Write
            //Debug.WriteLine($"SerialPortExtensions::SendCommand(): {command}");
            _serialPort.WriteToBuffer(command);

            // Read
            return _serialPort.ReadFromBuffer();
        }

        #endregion Sync Methods
    }
}