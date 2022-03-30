using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;


namespace Console_MVVMTesting.Views
{
    internal class ListLoadsPage : MyPage
    {
        public ListLoadsViewModel XamlViewModel { get; }

        public ListLoadsPage()
        {
            MyUtils.MyConsoleWriteLine($"[{System.DateTime.Now.ToString("HH:mm:ss.ff")}] " +
               $"ListLoadsPage::ListLoadsPage()  ({this.GetHashCode():x8})");

            XamlViewModel = Ioc.Default.GetService<ListLoadsViewModel>();
        }

        // pacz xaml
        //private void OnViewStateChanged(object sender, ListDetailsViewState e)
        //{
        //    System.Diagnostics.Debug.WriteLine($"ListLoadsPage::OnViewStateChanged()");
        //    if (e == ListDetailsViewState.Both)
        //    {
        //        ViewModel.EnsureItemSelected();
        //    }
        //}
    }
}
