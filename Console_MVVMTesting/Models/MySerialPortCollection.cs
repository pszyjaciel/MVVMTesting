using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Models
{
    internal class MySerialPortCollection
    {
        public int IdOfCollection { get; set; }

        public string NameOfCollection { get; set; }

        public ICollection<MySerialPort> myPorts { get; set; }

        public MySerialPortCollection()
        {

        }
    }
}
