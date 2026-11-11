using Microsoft.Extensions.DependencyInjection;
using PeakSWC.RemoteBlazorWebView;
using PeakSWC.RemoteBlazorWebView.Wpf;
using System;
using System.Net.Http;
using System.Windows;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    /// <summary>
    /// The sample app's window. [RemoteBlazor] generates InitializeRemoteBlazor(), which makes the view remote when the app is
    /// started with -u=&lt;server uri&gt; (and -i=&lt;guid&gt; for a fixed id) and leaves it local otherwise. The options used here:
    /// the window shows the link until a browser connects (the default), then a read-only mirror of what the browser is doing
    /// (Mirror); a browser refresh restarts the app on the same server and id (RestartOnRefresh); and the app closes when the
    /// browser goes away (ShutdownOnDisconnect).
    /// </summary>
    [RemoteBlazor(nameof(RemoteBlazorWebView), Mirror = true, RestartOnRefresh = true, ShutdownOnDisconnect = true)]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRemoteWpfBlazorWebView();
            serviceCollection.AddRemoteBlazorWebViewDeveloperTools();
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());

            InitializeComponent();
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            if (InitializeRemoteBlazor())
            {
                // Optional: a server whose gRPC endpoint is on another address says so here. Without this the server's own address is used.
                RemoteBlazorWebView.GrpcBaseUri = await RemoteBlazorWebView.GetGrpcBaseUriAsync(RemoteBlazorWebView.ServerUri);

                // Something of the app's own on top of the generated setup: say in the title who is driving.
                RemoteBlazorWebView.Connected += (_, connected) =>
                {
                    var user = connected.User.Length > 0 ? $"by user {connected.User} " : "";
                    Title += $" Controlled remotely {user}from ip address {connected.IpAddress}";
                };
            }

            RemoteBlazorWebView.RootComponents.Add<Microsoft.AspNetCore.Components.Web.HeadOutlet>("head::after");
            // Last: setting HostPage starts the view, and the server and id must be set before it.
            RemoteBlazorWebView.HostPage = @"wwwroot\index.html";
        }

        private async void TheMainWindow_Closed(object sender, EventArgs e)
        {
            await RemoteBlazorWebView.DisposeAsync();
        }
    }
}
