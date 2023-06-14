using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using PeakSWC.RemoteWebView;
using RemoteBlazorWebViewTutorial.Shared;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Options;

namespace BlazorWebView
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var switchMappings = new Dictionary<string, string>()
           {
               { "-u", "AppSettings:ServerUrl" },
               { "-i", "AppSettings:Id" },
           };
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddCommandLine(Environment.GetCommandLineArgs(), switchMappings);

            var Configuration = builder.Build();

            var appBuilder = RemoteBlazorWebViewAppBuilder.CreateDefault(args);

            appBuilder.Services.AddLogging();
            appBuilder.Services.AddScoped<HttpClient>();
            appBuilder.Services.Configure<AppSettings>(Configuration!.GetSection(nameof(AppSettings)));

            // register root component and selector
            appBuilder.RootComponents.Add<App>("#app");

            // Get run string
            var sc = new ServiceCollection();
            sc.Configure<AppSettings>(Configuration!.GetSection(nameof(AppSettings)));
            var sp = sc.BuildServiceProvider();
            var runString = sp.GetRequiredService<IOptions<AppSettings>>().Value;

            var app = appBuilder.Build(runString!.ServerUrl!, runString.Id);

            // customize window
            app.MainWindow!.SetTitle("Remote Blazor WebWindow Sample");
            app.MainWindow!.Disconnected += MainWindow_Disconnected;
            app.MainWindow!.ReadyToConnect += Program_ReadyToConnect;
            app.MainWindow!.Connected += Program_Connected;
            app.MainWindow.Refreshed += (s, e) => MainWindow_Refreshed(app.MainWindow);          
            
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
            };

            app.Run();

        }

        private static void Program_Connected(object? sender, ConnectedEventArgs e)
        {
            (sender as IBlazorWebView)?.NavigateToString($"User {e.User} is connected remotely from ip address {e.IpAddress}");
        }

        private static void Program_ReadyToConnect(object? sender, ReadyToConnectEventArgs e)
        {
            (sender as IBlazorWebView)?.NavigateToString($"<a href='{e.Url}app/{e.Id}' target='_blank'> {e.Url}app/{e.Id}</a>");
        }

        private static void MainWindow_Refreshed(RemoteBlazorWebViewWindow w)
        {
            w.Restart();
            Environment.Exit(0);
        }

        private static void MainWindow_Disconnected(object? sender, DisconnectedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
