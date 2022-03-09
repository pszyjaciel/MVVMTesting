using System;
using System.Collections.Generic;
using System.Text;


 namespace Console_MVVMTesting.Helpers
{
    internal static class MyUtils
    {
        private const string _defaultColor = "\x1B[37m";
        private const string _defaultBackgroundColor = "\x1B[40m";

        private static Dictionary<string, string> _myColorsDict;


        static MyUtils()
        {
            _myColorsDict = PrepareColorDictionary();
        }


        internal static void MyConsoleWriteLine(string myString)
        {
            MyConsoleWriteLine(_defaultColor, myString);
        }


        /// <summary>
        /// Function outputs myString to screen using the Foreground-color
        /// </summary>
        internal static void MyConsoleWriteLine(string myColorName, string myString)
        {
            bool colorFound = false;
            foreach (KeyValuePair<string, string> kvp in _myColorsDict)
            {
                if (kvp.Key == myColorName)
                {
                    Console.WriteLine(kvp.Value + myString + _defaultColor + _defaultBackgroundColor);
                    colorFound = true;
                    break;
                }
            }
            if (!colorFound)
            {
                Console.WriteLine(_defaultColor + myString + _defaultColor + _defaultBackgroundColor);
            }
            System.Diagnostics.Debug.WriteLine(myString);   // [VS] write to immediate window
        }


        /// <summary>
        /// Function outputs myString to screen using both FG and BG-colors
        /// </summary>
        internal static void MyConsoleWriteLineExt(string myForegroundColorName, string myBackgroundColorName, string myString)
        {
            string _myForegroundColorName = "";
            string _myBackgroundColorName = "";

            string _myForegroundColorCode = _defaultColor;
            string _myBackgroundColorCode = _defaultBackgroundColor;

            foreach (KeyValuePair<string, string> kvp in _myColorsDict)
            {
                if (kvp.Key == myForegroundColorName)
                {
                    _myForegroundColorName = kvp.Key;
                    _myForegroundColorCode = kvp.Value;
                    break;
                }
            }
            //Console.WriteLine($"_myForegroundColorName: {_myForegroundColorName}");

            foreach (KeyValuePair<string, string> kvp in _myColorsDict)
            {
                if (kvp.Key == myBackgroundColorName)
                {
                    _myBackgroundColorName = kvp.Key;
                    _myBackgroundColorCode = kvp.Value;
                    break;
                }
            }
            //Console.WriteLine($"_myBackgroundColorName: {_myBackgroundColorName}");

            Console.WriteLine(_myBackgroundColorCode + _myForegroundColorCode + myString + _defaultColor + _defaultBackgroundColor);
        }




        private static Dictionary<string, string> PrepareColorDictionary()
        {
            // \x1B czy \x1b to jeden huj
            _myColorsDict = new Dictionary<string, string>();
            _myColorsDict.Add("LWHITE", "\x1B[97m");
            _myColorsDict.Add("DWHITE", "\x1B[37m");
            _myColorsDict.Add("GREY", "\x1B[90m");   // nie wyswietla (ew. na czarno albo na rzaden)
            _myColorsDict.Add("BACKWHITE", "\x1B[47m");
            _myColorsDict.Add("BACKWHITE2", "\x1B[107m");

            _myColorsDict.Add("LGREEN", "\x1B[92m");
            _myColorsDict.Add("DGREEN", "\x1B[32m");
            _myColorsDict.Add("BACKLIGHTGREEN", "\x1B[102m");
            _myColorsDict.Add("BACKGREEN", "\x1B[42m");

            _myColorsDict.Add("LRED", "\x1B[91m");
            _myColorsDict.Add("DRED", "\x1B[31m");
            _myColorsDict.Add("BACKLIGHTRED", "\x1B[101m");
            _myColorsDict.Add("BACKRED", "\x1B[41m");

            _myColorsDict.Add("LBLUE", "\x1B[94m");
            _myColorsDict.Add("DBLUE", "\x1B[34m");
            _myColorsDict.Add("BACKLIGHTBLUE", "\x1B[104m");
            _myColorsDict.Add("BACKBLUE", "\x1B[44m");

            _myColorsDict.Add("LCYAN", "\x1B[96m");
            _myColorsDict.Add("DCYAN", "\x1B[36m");
            _myColorsDict.Add("BACKLIGHTCYAN", "\x1B[106m");
            _myColorsDict.Add("BACKCYAN", "\x1B[46m");

            _myColorsDict.Add("LYELLOW", "\x1B[93m");
            _myColorsDict.Add("DYELLOW", "\x1B[33m");
            _myColorsDict.Add("BACKLIGHTYELLOW", "\x1B[103m");
            _myColorsDict.Add("BACKYELLOW", "\x1B[43m");

            _myColorsDict.Add("LMAGENTA", "\x1B[95m");
            _myColorsDict.Add("DMAGENTA", "\x1B[35m");
            _myColorsDict.Add("BACKLIGHTMAGENTA", "\x1B[105m");
            _myColorsDict.Add("BACKMAGENTA", "\x1B[45m");

            _myColorsDict.Add("BLACK", "\x1B[30m");
            _myColorsDict.Add("BACKLIGHTGRAY", "\x1B[100m");
            _myColorsDict.Add("BACKBLACK", "\x1B[40m");

            return _myColorsDict;
        }



        internal static void DisplayStringInBytes2(string myString)
        {
            if (myString.Length == 0) return;

            //MyConsoleWriteLine("LWHITE", $"[{myString.Length}]: {myString}");

            int myValue, myIndex = 0;
            char[] chars = myString.ToCharArray();
            foreach (char myChar in chars)
            {
                //if ((myChar == 0x0a) || (myChar == 0x0d)) { myChar = 0x20; }
                myValue = Convert.ToInt32(myChar);
                if ((myChar == 0x0a) || (myChar == 0x0d))
                    MyConsoleWriteLine("DWHITE", $"[{myIndex:D4}] : [{myValue:X2}]");
                else
                    MyConsoleWriteLine("DWHITE", $"[{myIndex:D4}] : [{myValue:X2}] : {myChar}");
                //XAMLtbRX += $"{value:X2} ";
                myIndex++;
            }
        }



        internal static void DisplayStringInBytes(string myString)
        {
            if (myString.Length == 0) return;

            MyConsoleWriteLine("LWHITE", $"[{myString.Length}]: {myString}");

            int myValue;
            char[] chars = myString.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i % 0x10 == 0)
                {
                    System.Diagnostics.Debug.Write($"\n[{i:D4}] : ");
                }
                myValue = Convert.ToInt32(chars[i]);
                System.Diagnostics.Debug.Write($"[{myValue:X2}] ");   // [VS] write to immediate window
            }
            System.Diagnostics.Debug.Write("\n\n");
        }


        /// <summary>
        /// Returns incoming string as byte-string with new line
        /// </summary>
        internal static string GimmeStringInBytes(string myString)
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


        public static void MyWinUIConsoleWriteLine(ConsoleColor myForegroundColor, ConsoleColor myBackgroundColor, string something)
        {
            ConsoleColor currentBackground = Console.BackgroundColor;
            ConsoleColor currentForeground = Console.ForegroundColor;

            Console.ForegroundColor = myForegroundColor;
            Console.BackgroundColor = myBackgroundColor;

            Console.WriteLine(something);

            Console.ForegroundColor = currentForeground;
            Console.BackgroundColor = currentBackground;
        }


    }
}