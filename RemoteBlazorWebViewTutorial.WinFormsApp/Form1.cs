// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
using System.Windows.Documents;
using System.Windows.Input;
using PeakSWC.RemoteWebView;

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
               { "-r", "AppSettings:IsRestarting" },
           };

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddCommandLine(Environment.GetCommandLineArgs(), switchMappings);

            var Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            serviceCollection.Configure<AppSettings>(Configuration!.GetSection(nameof(AppSettings)));
            InitializeComponent();

            blazorWebView1.Services = serviceCollection.BuildServiceProvider();

            var runString = blazorWebView1.Services.GetRequiredService<IOptions<AppSettings>>().Value;

            blazorWebView1.ServerUri = runString.ServerUrl;
            blazorWebView1.Id = runString.Id;
            blazorWebView1.IsRestarting = runString.IsRestarting;
            blazorWebView1.HostPage = @"wwwroot\index.html";
            
            blazorWebView1.RootComponents.Add<App>("#app");
            if (runString.ServerUrl == null)
            {
                blazorWebView1.Visible = true;
                linkLabel1.Visible = false;
            }
            else
            {
                blazorWebView1.Visible = false;
                linkLabel1.Visible = !blazorWebView1.IsRestarting;
                linkLabel1.Text = $"{blazorWebView1.ServerUri}app/{blazorWebView1.Id}";
            }
            blazorWebView1.Refreshed += BlazorWebView1_Refreshed;
        }

        private void BlazorWebView1_Refreshed(object? sender, RefreshedEventArgs e)
        {
            blazorWebView1.BeginInvoke((Action)(() =>
            {
                blazorWebView1.Restart();
                Close();
            }));

        }

        private async void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.Visible = false;
            await blazorWebView1.StartBrowser();
        }

        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
                Clipboard.SetText(linkLabel1.Text);
        }
    }
}
