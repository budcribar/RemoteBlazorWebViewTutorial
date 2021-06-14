using System;
using System.Linq;
using System.Windows;
using RemoteBlazorWebView.Wpf;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using RemoteBlazorWebViewTutorial.Shared;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public partial class MainWindow : Window
    {
        private bool initialized = false;
        public RunString Command { get; set; } = new RunString();

        public MainWindow()
        {
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

                IBlazorWebView? rbwv = RemoteBlazorWebView as IBlazorWebView;
                if (rbwv == null) return;
               
                rbwv.Unloaded += (x, y) => { 
                    Application.Current.Dispatcher.Invoke(() => {
                        rbwv.Restart();
                        Close();
                    });
                };
            }
        }

       
       
    }
}
