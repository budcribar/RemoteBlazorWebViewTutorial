using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using PeakSWC.RemoteWebView;
using RemoteBlazorWebViewTutorial.Shared;

namespace Photino.Blazor.Sample
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var appBuilder = RemotePhotinoBlazorAppBuilder.CreateDefault(args);

            appBuilder.Services.AddLogging();
            appBuilder.Services.AddScoped<HttpClient>();

            // register root component and selector
            appBuilder.RootComponents.Add<App>("#app");

            var app = appBuilder.Build();

            // customize window
            app.MainWindow.SetTitle("Remote Photino Blazor Sample");

            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                app.MainWindow.OpenAlertWindow("Fatal exception", error.ExceptionObject.ToString());
            };

            app.Run();

        }
    }
}
