using System;
using System.Windows;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
                {
                    MessageBox.Show(error.ExceptionObject.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                };

            // The server (-u=) and id (-i=) on the command line are read by the code [RemoteBlazor] generates for MainWindow.
            MainWindow wnd = new();
            wnd.Show();
        }
    }
}
