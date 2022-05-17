// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using PeakSWC.RemoteWebView;
using Microsoft.Extensions.DependencyInjection;
using RemoteBlazorWebViewTutorial.Shared;
using System;
using System.Net.Http;
using System.Windows.Forms;
using PeakSWC.RemoteBlazorWebView.WindowsForms;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Web;
using PeakSWC.RemoteBlazorWebView;
using System.Threading.Tasks;

namespace BlazorWinFormsApp
{
    public partial class Form1 : Form
    {

        public Form1()
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

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRemoteWindowsFormsBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            serviceCollection.Configure<AppSettings>(Configuration!.GetSection(nameof(AppSettings)));
            InitializeComponent();

            blazorWebView1.Services = serviceCollection.BuildServiceProvider();

            var runString = blazorWebView1.Services.GetRequiredService<IOptions<AppSettings>>().Value;

            blazorWebView1.ServerUri = runString.ServerUrl;
            blazorWebView1.Id = runString.Id;
            blazorWebView1.HostPage = @"wwwroot\index.html";            
            blazorWebView1.RootComponents.Add<App>("#app");
            blazorWebView1.RootComponents.Add<HeadOutlet>("head::after");

            blazorWebView1.Refreshed += BlazorWebView1_Refreshed;
			blazorWebView1.Disconnected += BlazorWebView1_Disconnected;
            blazorWebView1.Connected += BlazorWebView1_Connected;
            blazorWebView1.ReadyToConnect += BlazorWebView1_ReadyToConnect;
        }

        private void BlazorWebView1_ReadyToConnect(object? sender, ReadyToConnectEventArgs e)
        {
            blazorWebView1.NavigateToString($"<a href='{e.Url}app/{e.Id}' target='_blank'> {e.Url}app/{e.Id}</a>");
        }

        private void BlazorWebView1_Connected(object? sender, ConnectedEventArgs e)
        {
            blazorWebView1.WebView.CoreWebView2.Navigate($"{e.Url}mirror/{e.Id}");
            var user = e.User.Length > 0 ? $"by user {e.User.Length}" : "";
            Text = Text + $" Controlled remotely {user}from ip address {e.IpAddress}";
        }

        private void BlazorWebView1_Disconnected(object? sender, DisconnectedEventArgs e)
		{
            Application.Exit();
		}

        private void BlazorWebView1_Refreshed(object? sender, RefreshedEventArgs e)
        {
            blazorWebView1.Restart();
            Close();
        }
    }
}
