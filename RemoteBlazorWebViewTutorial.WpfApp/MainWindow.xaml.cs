using System;
using System.Linq;
using System.Windows;
using RemoteBlazorWebView.Wpf;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public partial class MainWindow : Window
    {
        private bool initialized = false;

        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            DataContext = this;
            ParseRunstring();
            InitializeComponent();
        }

        public Uri? ServerUri { get; set; }
        public Guid Id { get; set; } = Guid.Parse("d8d19338-3d66-4942-912b-5b3103efa177");
        public bool IsRestarting { get; set; } = false;
   
        private void ParseRunstring()
        {
            IsRestarting = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-r")) != null;

            // -u=https://localhost:443 -i=9BFD9D43-0289-4A80-92D8-6E617729DA12
            try
            {
                var u = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-u"));
                if (u != null)
                   ServerUri = new Uri(u.Split("=")[1]);
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
                this.initialized = true;

                var rbwv = RemoteBlazorWebView;// as IBlazorWebView;
                if (rbwv == null) return;
                rbwv.Loaded += (x, y) => 
                    //MessageBox.Show("Loaded");
                rbwv.Unloaded += (x, y) => Restart();
            }
        }

        private void Restart()
        {
            var psi = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName
            };
            psi.ArgumentList.Add($"-u={ServerUri}");
            psi.ArgumentList.Add($"-i={Id}");
            psi.ArgumentList.Add($"-r=true");

            Process.Start(psi);
            Application.Current.Dispatcher.Invoke(Close);
        }
       
    }
}
