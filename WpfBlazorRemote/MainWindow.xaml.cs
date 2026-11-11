using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PeakSWC.RemoteBlazorWebView;

namespace WpfBlazorRemote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. [RemoteBlazor] generates InitializeRemoteBlazor(), which sets the server and the id
    /// and, with ShareControl, shows the app in this window with the person at the PC in control (see ConvertToRemoteBlazor.md).
    /// A remote user joins at the link the session bar's Copy link gives and is handed control from the bar. RestartOnRefresh
    /// restarts the app on the same session when Restart is pressed in the bar. A -u=&lt;server uri&gt; on the command line
    /// overrides ServerUri; --unattended gives the remote user control on joining, and --voice adds a voice call.
    /// </summary>
    [RemoteBlazor(nameof(RemoteBlazorWebView), Mode = RemoteBlazorMode.Remote, ServerUri = "https://localhost:5001",
        ShareControl = true, RestartOnRefresh = true)]
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
