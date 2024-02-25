using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PeakSWC.RemoteWebView;
using PeakSWC.RemoteBlazorWebView.Wpf;
using RemoteBlazorWebViewTutorial.Shared;
using System.Net.Http;
using System.Windows;
using PeakSWC.RemoteBlazorWebView;

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
            serviceCollection.AddSingleton<IBlazorWebView>(s => RemoteBlazorWebView);
            serviceCollection.AddScoped<HttpClient>();
            serviceCollection.AddRemoteBlazorWebViewDeveloperTools();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            InitializeComponent();
            RemoteBlazorWebView.Id = Command.Id;
            RemoteBlazorWebView.RootComponents.Add<Microsoft.AspNetCore.Components.Web.HeadOutlet>("head::after");
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
            RemoteBlazorWebView.WebViewManager.Navigate($"{e.Url}mirror/{e.Id}");
            var user = e.User.Length > 0 ? $"by user {e.User}" : "";
            Title += $" Controlled remotely {user} from ip address {e.IpAddress}";
        }

        private void Rbwv_Disconnected(object? sender, DisconnectedEventArgs e) => 
            Application.Current.Shutdown();
    }
}
