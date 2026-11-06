using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PeakSWC.RemoteBlazorWebView;

namespace WpfBlazorRemote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. [RemoteBlazor] generates InitializeRemoteBlazor(), which sets the server, the id and
    /// the link handler (see ConvertToRemoteBlazor.md). A -u=&lt;server uri&gt; on the command line overrides ServerUri.
    /// </summary>
    [RemoteBlazor(nameof(RemoteBlazorWebView), Mode = RemoteBlazorMode.Remote, ServerUri = "https://localhost:5001",
        RestartOnRefresh = true)]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRemoteWpfBlazorWebView();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            InitializeRemoteBlazor();
        }
    }
}
