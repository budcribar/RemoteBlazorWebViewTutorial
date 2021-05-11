using System;
using System.Linq;
using System.Windows;
//using PeakSwc.RemoteableWebWindows;



namespace RemoteBlazorWebViewTutorial.WpfApp
{
    //  <!--xmlns:blazor="clr-namespace:RemoteBlazorWebView.Wpf;assembly=PeakSWC.RemoteBlazorWebView.Wpf"-->
    // add usings here
    //using BlazorWebView.Wpf;
    //using BlazorWebView;
    using System.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using Microsoft.AspNetCore.Components.WebView.Wpf;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDisposable? disposable;
        private bool initialized = false;

        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
          
            InitializeComponent();
            
            //RemoteBlazorWebView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(RemoteBlazorWebViewTutorial.WpfApp.Main) });
        }

        private Uri? Uri { get; set; } = null;
        private Guid Id { get; set; } = default(Guid);

        private void ParseRunstring()
        {
            // -u=https://localhost:443 -i=9BFD9D43-0289-4A80-92D8-6E617729DA12
            try
            {
                var u = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-u"));
                if (u != null)
                   Uri = new Uri(u.Split("=")[1]);
            }
            catch (Exception) { }
            try
            {
                var i = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-i"));
                if (i != null)
                    Id = Guid.Parse(i.Split("=")[1]);
            }
            catch (Exception) { Id = Guid.NewGuid(); }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!this.initialized)
            {
                ParseRunstring();
                this.initialized = true;
                //this.disposable = this.RemoteBlazorWebView.Run<Startup>("wwwroot/index.html", null, Uri, Id);

                //this.RemoteBlazorWebView.OnDisconnected += (s, e) => Restart();
                // this.RemoteBlazorWebView.OnConnected += (s, e) => { this.RemoteBlazorWebView.ShowMessage("Title", "Hello World"); };
            }
        }

        private void Restart()
        {
            Process.Start(new ProcessStartInfo { FileName = Process.GetCurrentProcess().MainModule?.FileName, Arguments = $"-u={Uri} -i={Id}" });
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
    public partial class Main { }
}
