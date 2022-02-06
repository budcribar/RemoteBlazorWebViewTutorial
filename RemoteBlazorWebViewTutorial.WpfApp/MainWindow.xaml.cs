using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PeakSWC.RemoteWebView;
using PeakSWC.RemoteBlazorWebView.Wpf;
using RemoteBlazorWebViewTutorial.Shared;
using System;
using System.Net.Http;
using System.Windows;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public partial class MainWindow : Window
    {
        private bool initialized = false;
        public AppSettings Command { get; init; }
        public MainWindow(IOptions<AppSettings> settings)
        {
            Command = settings.Value;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            InitializeComponent();
           
            RemoteBlazorWebView.Id = Command.Id;
            RemoteBlazorWebView.RootComponents.Add<HeadOutlet>("head::after");
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!this.initialized)
            {
                this.initialized = true;

                if (RemoteBlazorWebView is not IBlazorWebView rbwv) return;
                rbwv.Disconnected += Rbwv_Disconnected;
                rbwv.Connected += Rbwv_Connected;
                rbwv.ReadyToConnect += Rbwv_ReadyToConnect;
                rbwv.Refreshed += (_, _) =>
                {
                    rbwv.Restart();
                    Close();
                };
            }
        }

        private void Rbwv_ReadyToConnect(object? sender, ReadyToConnectEventArgs e)
        {
            (sender as IBlazorWebView)?.NavigateToString($"<a href='{e.Url}app/{e.Id}' target='_blank'> {e.Url}app/{e.Id}</a>");
        }

        private void Rbwv_Connected(object? sender, ConnectedEventArgs e)
        {
            (sender as IBlazorWebView)?.NavigateToString($"User {e.User} is connected remotely from ip address {e.IpAddress}");
        }

        private void Rbwv_Disconnected(object? sender, DisconnectedEventArgs e) => Application.Current.Shutdown();
    }
}
