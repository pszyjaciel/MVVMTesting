using Console_MVVMTesting.Helpers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Console_MVVMTesting.Models
{
    public class ConnectionItem : ObservableObject
    {
        private const string consoleColor = "LEMON";
        MyUtils mu = new MyUtils();


        #region Property Name
        public const string NamePropertyName = "Name";  // nazwa do poprawy
        /// <summary>
        /// Free text name of connection
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged(NamePropertyName);

                    //TerminalHeaderText = GetFormatedTerminalHeaderText();
                    //HelpHeaderText = GetFormatedHelpHeaderText();
                    //RaisePropertyChanged(TerminalHeaderTextPropertyName);
                    //RaisePropertyChanged(HelpHeaderTextPropertyName);
                }
            }
        }
        #endregion




        #region Property Host
        public const string PropertyNameHost = "Host";
        /// <summary>
        /// Host is either IpAddress or hostname
        /// </summary>
        private string _host; 
        public string Host
        {
            get { return _host; }
            set
            {
                if (value != _host)
                {
                    _host = value;
                    OnPropertyChanged(PropertyNameHost);
                    //TerminalHeaderText = GetFormatedTerminalHeaderText();
                    //HelpHeaderText = GetFormatedHelpHeaderText();
                }
            }
        }
        #endregion



        #region Property Port
        public const string PortPropertyName = "Port";
        /// <summary>
        /// Port is the port number 0..65535
        /// </summary>
        private UInt16 _port;
        public UInt16 Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                    OnPropertyChanged(PortPropertyName);
                    //TerminalHeaderText = GetFormatedTerminalHeaderText();
                    //HelpHeaderText = GetFormatedHelpHeaderText();
                }
            }
        }
        #endregion


        

        #region Property Pinned
        public const string PinnedPropertyName = "Pinned";
        /// <summary>
        /// Pinned means show as default
        /// </summary>
        private bool _pinned; 
        public bool Pinned
        {
            get { return _pinned; }
            set
            {
                if (value != _pinned)
                {
                    _pinned = value;
                    OnPropertyChanged(PinnedPropertyName);
                }
            }
        }
        #endregion


        
        

        #region Property ConnectTime
        public const string ConnectTimePropertyName = "ConnectTime";
        /// <summary>
        /// Timestamp of last connect
        /// </summary>
        private DateTime _connectTime; 
        public DateTime ConnectTime
        {
            get { return _connectTime; }
            set
            {
                if (value != _connectTime)
                {
                    _connectTime = value;
                    OnPropertyChanged(ConnectTimePropertyName);
                }
            }
        }
        #endregion




        #region Property HelpSearches
        /// <summary>
        /// History list of help searches for this connection
        /// </summary>
        public List<string> HelpSearches { get; set; }
        #endregion


        #region Property TerminalCommands
        /// <summary>
        /// History list of terminal commands for this connection
        /// </summary>
        //public List<string> TerminalCommands { get; set; }
        public ObservableCollection<string> TerminalCommands;
        #endregion




        public ConnectionItem()
        {
            mu.MyConsoleWriteLine(consoleColor, $"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"ConnectionItem::ConnectionItem()  ({this.GetHashCode():x8})");

            Name = "";
            Host = "";
            Port = 0;
            Pinned = false;
            ConnectTime = DateTime.UtcNow;

            HelpSearches = new List<string>();
            TerminalCommands = new ObservableCollection<string>();
        }
    }
}
