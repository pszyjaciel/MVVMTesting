using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_MVVMTesting.ViewModels
{
    class MyTrash
    {
    }
}



// https://docs.microsoft.com/en-us/visualstudio/extensibility/launch-visual-studio-dte?view=vs-2022

//public void ClearImmediateWindow()
//{

    //var dte = GetCurrent();


    //Window immediateWindow = dte.Windows.Item( { ECB7191A - 597B - 41F5 - 9843 - 03A4CF275DDE } );
    //immediateWindow.Activate();
    //CommandWindow cw = immediateWindow.Object as CommandWindow;
    //cw.SendInput(&quot; DateTime.Now & quot;, false);

    //var obj = System.Runtime.InteropServices.Marshal.GetObject("VisualStudio.DTE.10.0");

    //EnvDTE.Window refDTE;
    //EnvDTE.Window currentActiveWindow = refDTE.ActiveWindow;
    //refDTE.DTE.ActiveDocument

    //    //Windows.Item("{ECB7191A-597B-41F5-9843-03A4CF275DDE}").Activate();
    //refDTE.ExecuteCommand("Edit.SelectAll");
    //currentActiveWindow.Activate();
//}

//[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
//[ComImport, Guid("ECB7191A-597B-41F5-9843-03A4CF275DDE")]
//internal interface IWindowNative
//{
//    string obj = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown("VisualStudio.DTE.10.0");
//    //IntPtr WindowHandle { get; }
//}

//Type type = Type.GetTypeFromProgID("VisualStudio.DTE.16.0");
//if (type == null)
//{
//    throw new Exception("Visual Studio 2008 cannot be loaded");
//}
//Object obj = System.Activator.CreateInstance(type, true);
//DTE m_dte = (DTE)obj;


//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.ActiveWindow.WindowState}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.MainWindow.Project.FullName}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.MainWindow.Project.FileName}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.MainWindow.Project.Name}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.MainWindow.Project.Saved}");
////MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.Solution.FileName}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.Solution.FullName}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.Version}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.ActiveDocument.ProjectItem}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.ActiveDocument.FullName}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.ActiveDocument.Name}");

//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.ActiveWindow.HWnd}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.ActiveWindow.Document}");
//MyUtils.MyConsoleWriteLine($"EastTesterViewModel::EastTesterViewModel(): {m_dte.ActiveWindow.Document.Language}");


//EnvDTE.Solution m_dteSolution = MyUtils.Call(() => (m_dte.Solution));
//m_dteSolution.Close();  

//EnvDTE.Projects rootProjects = MyUtils.call(() => (m_dteSolution.Projects));
