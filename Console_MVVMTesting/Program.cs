using System;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Console_MVVMTesting.Helpers;
using System.Collections.Generic;
using System.Net;
using Console_MVVMTesting.Services;
using Console_MVVMTesting.ViewModels;
using Console_MVVMTesting.Views;

namespace Console_MVVMTesting
{
    internal class Program
    {
        public static ShellPage Content1 { get; internal set; }
        public static EastTesterPage Content2 { get; internal set; }
        public static LCSocketPage Content3 { get; internal set; }
        public static UserReceiverPage Content4 { get; internal set; }
        public static UserSenderPage Content5 { get; internal set; }
        public static UserReceiver2Page Content6 { get; internal set; }
        public static UserSender2Page Content7 { get; internal set; }
        public static ProductionPage Content8 { get; internal set; }



        private static Semaphore _serialSemaphore;

        internal static void Activate()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
              $"Program::Activate() ({typeof(SerialPortExtensions).GetHashCode():x8})");
        }


        public static async Task ReadPort(SerialPort port)
        {
            await SerialPortExtensions.ReadLineAsync(port);
        }




        private static IServiceProvider ConfigureMyServices()
        {
            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"Program::ConfigureServices(): start of method" +
                $"({typeof(SerialPortExtensions).GetHashCode():x8})");


            // TODO WTS: Register your services, viewmodels and pages here
            ServiceCollection services = new ServiceCollection();

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();     // pacztu
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            services.AddTransient<ProductionPage>();
            services.AddSingleton<ProductionViewModel>();

            services.AddTransient<UserReceiverPage>();
            services.AddSingleton<UserReceiverViewModel>();

            services.AddTransient<UserSenderPage>();
            services.AddSingleton<UserSenderViewModel>();

            services.AddTransient<UserReceiver2Page>();
            services.AddSingleton<UserReceiver2ViewModel>();

            services.AddTransient<UserSender2Page>();
            services.AddSingleton<UserSender2ViewModel>();


            services.AddTransient<EastTesterPage>();
            services.AddSingleton<EastTesterViewModel>();

            services.AddTransient<LCSocketPage>();
            services.AddSingleton<LCSocketViewModel>();

            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"Program::ConfigureServices(): end of method" +
                $"({typeof(SerialPortExtensions).GetHashCode():x8})");


            return services.BuildServiceProvider();

        }


        // http://sheepsqueezers.com/media/presentations/dotNetLectureSeries_CSharpProgrammingIII.pdf tesz fajne

        /// ////////////////////////////////////
        static void Main(string[] args)
        {
            MySerialPort msp = new MySerialPort();

            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"==========> Program::Main(): start of program <========== " +
                $"({typeof(SerialPortExtensions).GetHashCode():x8})");


            Ioc.Default.ConfigureServices(ConfigureMyServices());
            IActivationService activationService = Ioc.Default.GetService<IActivationService>();
            activationService.ActivateAsync(args);



            MyUtils.MyConsoleWriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}] " +
                $"==========> Program::Main(): end of program <========== " +
                $"({typeof(SerialPortExtensions).GetHashCode():x8})");


            //Simulate performing work here...main program must not end before
            // thread finishes or you will lose your results!
            Console.ReadLine();     // musi ma byc, o ile nie zablokuje sie gdzies w ViewModelach 

        }


    }


}
