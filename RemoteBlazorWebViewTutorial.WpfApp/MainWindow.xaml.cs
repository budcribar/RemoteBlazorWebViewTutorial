using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PeakSwc.RemoteableWebWindows;



namespace RemoteBlazorWebViewTutorial.WpfApp
{
    // add usings here
    using BlazorWebView.Wpf;
    using BlazorWebView;
    using System.Threading;
    using Microsoft.JSInterop;
    using System.Reflection;
    using System.Diagnostics;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDisposable? disposable;
        private bool initialized = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private Uri? Uri { get; set; } = null;
        private Guid Id { get; set; } = default(Guid);

        private void ParseRunstring()
        {
            // -u=https://localhost:443 -i=9BFD9D43-0289-4A80-92D8-6E617729DA12
            try
            {
                Uri = new Uri(Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-u")).Split("=")[1]);
            }
            catch (Exception) { }
            try
            {
                Id = Guid.Parse(Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-i")).Split("=")[1]);
            }
            catch (Exception) { Id = Guid.NewGuid(); }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!this.initialized)
            {
                ParseRunstring();
                this.initialized = true;
                this.disposable = this.RemoteBlazorWebView.Run<Startup>("wwwroot/index.html", null, Uri, Id);

                this.RemoteBlazorWebView.OnDisconnected += (s, e) => Restart();
                // this.RemoteBlazorWebView.OnConnected += (s, e) => { this.RemoteBlazorWebView.ShowMessage("Title", "Hello World"); };
            }
        }

        private void Restart()
        {
            // TODO
            Process.Start(new ProcessStartInfo {  FileName= Process.GetCurrentProcess().MainModule.FileName, Arguments =$"-u={Uri} -i={Id}"});
            Application.Current.Dispatcher.Invoke(Close);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.disposable != null)
            {
                this.disposable.Dispose();
                this.disposable = null;
            }
        }

       
    }
}
