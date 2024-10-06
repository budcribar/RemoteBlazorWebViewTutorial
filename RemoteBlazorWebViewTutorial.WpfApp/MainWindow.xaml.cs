using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PeakSWC.RemoteWebView;
using PeakSWC.RemoteBlazorWebView.Wpf;
using RemoteBlazorWebViewTutorial.Shared;
using System.Net.Http;
using System.Windows;
using PeakSWC.RemoteBlazorWebView;
using System;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public partial class MainWindow : Window
    {
        public AppSettings Command { get; init; }
        public MainWindow(IOptions<AppSettings> settings)
        {       
            Command = settings.Value;
            var serviceCollection = new ServiceCollection();
           
            serviceCollection.AddRemoteWpfBlazorWebView();
            serviceCollection.AddRemoteBlazorWebViewDeveloperTools();
            //serviceCollection.AddSingleton<IBlazorWebView>(s => RemoteBlazorWebView);
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
           
            InitializeComponent();
            RemoteBlazorWebView.Id = Command.Id;

        }

        private void Rbwv_ReadyToConnect(object? sender, ReadyToConnectEventArgs e)
        {
            (sender as IBlazorWebView)?.NavigateToString($"<a href='{e.Url}app/{e.Id}' target='_blank'> {e.Url}app/{e.Id}</a>");
        }

        private void Rbwv_Connected(object? sender, ConnectedEventArgs e)
        {
            RemoteBlazorWebView.WebView.CoreWebView2.Navigate($"{e.Url}mirror/{e.Id}");
            var user = e.User.Length > 0 ? $"by user {e.User}" : "";
            Title += $" Controlled remotely {user} from ip address {e.IpAddress}";
        }

        private void Rbwv_Disconnected(object? sender, DisconnectedEventArgs e) => Close();

        private async void Window_Initialized(object sender, System.EventArgs e)
        {
            var gppcUri = await RemoteBlazorWebView.GetGrpcBaseUriAsync(Command.ServerUrl);

            RemoteBlazorWebView.GrpcBaseUri = gppcUri;
        
            RemoteBlazorWebView.RootComponents.Add<Microsoft.AspNetCore.Components.Web.HeadOutlet>("head::after");
            RemoteBlazorWebView.Disconnected += Rbwv_Disconnected;
            RemoteBlazorWebView.Connected += Rbwv_Connected;
            RemoteBlazorWebView.ReadyToConnect += Rbwv_ReadyToConnect;

            RemoteBlazorWebView.Refreshed += (_, _) =>
            {
                RemoteBlazorWebView.Restart();
                Close();
            };
            RemoteBlazorWebView.HostPage = @"wwwroot\index.html";

        }

        private async void TheMainWindow_Closed(object sender, EventArgs e)
        {
            await RemoteBlazorWebView.DisposeAsync();
        }
    }
}
