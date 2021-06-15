using System;
using System.Linq;
using System.Windows;
using RemoteBlazorWebView.Wpf;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using RemoteBlazorWebViewTutorial.Shared;
using PeakSwc.RemoteableWebWindows;
using System.Windows.Navigation;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public partial class MainWindow : Window
    {
        private bool initialized = false;
        public RunString Command { get; set; } = new RunString();
        public Visibility ShowHyperlink { get; set; }
        public Visibility ShowWebView { get; set; }
      

        public MainWindow()
        {
            ShowHyperlink = Command.ServerUri == null ? Visibility.Hidden : Visibility.Visible;
            ShowWebView = Command.ServerUri == null ? Visibility.Visible : Visibility.Hidden;       
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            DataContext = this;
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!this.initialized)
            {
                this.initialized = true;

                if (RemoteBlazorWebView is not IBlazorWebView rbwv) return;

                if (rbwv.ServerUri != null)
                {
                    HyperLink.NavigateUri = new Uri($"{rbwv.ServerUri}app/{rbwv.Id}");
                    LinkText.Text = $"{rbwv.ServerUri}app/{rbwv.Id}";
                }
               
                rbwv.Unloaded += (x, y) => { 
                    Application.Current.Dispatcher.Invoke(() => {
                        rbwv.Restart();
                        Close();
                    });
                };
            }
        }

        private void Hyperlink_Click(object sender, RequestNavigateEventArgs e)
        {
            ShowHyperlink = Visibility.Hidden;
            RemotableWebWindow.StartBrowser(RemoteBlazorWebView);
        }
    }
}
