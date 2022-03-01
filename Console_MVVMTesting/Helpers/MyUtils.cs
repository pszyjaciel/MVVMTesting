using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.Helpers
{
    internal class MyUtils
    {
        private const string defaultColor = "DWHITE";
        private Dictionary<string, string> myColorsDict;

        public MyUtils()
        {
            myColorsDict = new Dictionary<string, string>();
            myColorsDict.Add("LWHITE", "\x1B[97m");
            myColorsDict.Add("DWHITE", "\x1B[37m");
            myColorsDict.Add("GREY", "\x1B[90m");
            myColorsDict.Add("LGREEN", "\x1B[92m");
            myColorsDict.Add("DGREEN", "\x1B[32m");
            myColorsDict.Add("LRED", "\x1B[91m");
            myColorsDict.Add("DRED", "\x1B[31m");
            myColorsDict.Add("LBLUE", "\x1B[94m");
            myColorsDict.Add("DBLUE", "\x1B[34m");
            myColorsDict.Add("LCYAN", "\x1B[96m");
            myColorsDict.Add("DCYAN", "\x1B[36m");
            myColorsDict.Add("BYELLOW", "\x1b[1;93m");  // huja tam
            myColorsDict.Add("LEMON", "\x1B[93m");
            myColorsDict.Add("ORANGE", "\x1B[33m");
            myColorsDict.Add("PINK", "\x1B[95m");
            myColorsDict.Add("MAGENTA", "\x1B[35m");
            myColorsDict.Add("BLACK", "\x1B[30m");
        }

        public void MyConsoleWriteLine(string myString)
        {
            MyConsoleWriteLine(defaultColor, myString);
        }


        public void MyConsoleWriteLine(string myColorName, string myString)
        {
            foreach (KeyValuePair<string, string> kvp in myColorsDict)
            {
                if (kvp.Key == myColorName)
                {
                   System.Console.WriteLine(kvp.Value + myString + "\x1B[37m");
                }
            }
        }

        internal Solution Call(Func<object> p)
        {
            Console.WriteLine("MyUtils::call()");

            return new Solution();
        }



        /// <summary>
        /// Returns incoming string as byte-string with new line
        /// </summary>
        internal string GimmeStringInBytes(string myString)
        {
            if (myString.Length == 0) return "";
            StringBuilder stringBuilder = new StringBuilder();
            int value;
            char[] chars = myString.ToCharArray();
            foreach (var myChar in chars)
            {
                value = Convert.ToInt32(myChar);
                stringBuilder.Append($"{value:X2} ");
            }
            stringBuilder.Append($"\r\n");
            MyConsoleWriteLine("LWHITE", stringBuilder.ToString());
            return stringBuilder.ToString();
        }


        internal void DisplayStringInBytes(string myString)
        {
            this.MyConsoleWriteLine($"MyUtils::DisplayStringInBytes(): myString.Length: {myString.Length}");

            if (myString.Length == 0) return;
            //MyConsoleWriteLine("LWHITE", $"{myString.Length}");

            int value;
            char[] chars = myString.ToCharArray();
            foreach (var myChar in chars)
            {
                value = Convert.ToInt32(myChar);
                MyConsoleWriteLine("LWHITE", $"{value:X2} : {myChar}");
                this.MyConsoleWriteLine($"{value:X2} : {myChar}");

                //XAMLtbRX += $"{value:X2} ";
            }
            Console.WriteLine();
        }


    }
}
