using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PeakSWC.RemoteableWebView;
using RemoteBlazorWebViewTutorial.Shared;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public partial class MainWindow : Window
    {
        private bool initialized = false;
        public AppSettings Command { get; init; }
        public Visibility ShowWebView { get; set; }
        public ViewModel ViewModel { get; set; } = new();

        public MainWindow(IOptions<AppSettings> settings)
        {
            Command = settings.Value;
            ViewModel.HyperLinkVisible = (Command.ServerUrl != null && !Command.IsRestarting) ? Visibility.Visible : Visibility.Hidden;
            ShowWebView = Command.ServerUrl == null ? Visibility.Visible : Visibility.Hidden; 
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            DataContext = ViewModel;
            InitializeComponent();
           
            RemoteBlazorWebView.Id = Command.Id;
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

                rbwv.Unloaded += (_, _) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        rbwv.Restart();
                        Close();
                    });
                };
            }
        }

        private async void Hyperlink_Click(object sender, RequestNavigateEventArgs e)
        {
            this.ViewModel.HyperLinkVisible = Visibility.Hidden;

            await RemoteBlazorWebView.StartBrowser();
        }

        private void LinkText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                Clipboard.SetText(HyperLink.NavigateUri.ToString());
        }
    }
}
