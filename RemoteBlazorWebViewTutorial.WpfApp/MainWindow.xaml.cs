using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PeakSWC.RemoteWebView;
using PeakSWC.RemoteBlazorWebView.Wpf;
using RemoteBlazorWebViewTutorial.Shared;
using System.Net.Http;
using System.Windows;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public partial class MainWindow : Window
    {
        public AppSettings Command { get; init; }
        public MainWindow(IOptions<AppSettings> settings)
        {
            Command = settings.Value;
            var serviceCollection = new ServiceCollection();
           
            PeakSWC.RemoteBlazorWebView.BlazorWebViewServiceCollectionExtensions.AddWpfBlazorWebView(serviceCollection);
            
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            InitializeComponent();
           
            RemoteBlazorWebView.Id = Command.Id;
            RemoteBlazorWebView.RootComponents.Add<HeadOutlet>("head::after");
            RemoteBlazorWebView.Disconnected += Rbwv_Disconnected;
            RemoteBlazorWebView.Connected += Rbwv_Connected;
            RemoteBlazorWebView.ReadyToConnect += Rbwv_ReadyToConnect;
            RemoteBlazorWebView.Refreshed += (_, _) =>
            {
                RemoteBlazorWebView.Restart();
                Close();
            };
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
