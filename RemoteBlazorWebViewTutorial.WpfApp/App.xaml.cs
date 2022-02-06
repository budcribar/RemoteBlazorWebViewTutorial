using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RemoteBlazorWebViewTutorial.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
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
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Configure<AppSettings>(Configuration!.GetSection(nameof(AppSettings)));
            var services = serviceCollection.BuildServiceProvider();

            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
                {
                    MessageBox.Show(error.ExceptionObject.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                };

            MainWindow wnd = new(services.GetRequiredService<IOptions<AppSettings>>());
            wnd.Show();
        }
    }
}
