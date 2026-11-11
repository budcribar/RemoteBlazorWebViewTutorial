// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using PeakSWC.RemoteBlazorWebView;
using PeakSWC.RemoteBlazorWebView.WindowsForms;
using RemoteBlazorWebViewTutorial.Shared;
using System;
using System.Net.Http;
using System.Windows.Forms;

namespace BlazorWinFormsApp
{
    /// <summary>
    /// The sample app's form. [RemoteBlazor] generates InitializeRemoteBlazor(), which makes the view remote when the app is
    /// started with -u=&lt;server uri&gt; (and -i=&lt;guid&gt; for a fixed id) and leaves it local otherwise. The options used here:
    /// the form shows the link until a browser connects (the default), then a read-only mirror of what the browser is doing
    /// (Mirror); a browser refresh restarts the app on the same server and id (RestartOnRefresh); and the app closes when the
    /// browser goes away (ShutdownOnDisconnect).
    /// </summary>
    [RemoteBlazor(nameof(blazorWebView1), Mirror = true, RestartOnRefresh = true, ShutdownOnDisconnect = true)]
    public partial class Form1 : Form
    {
        public Form1()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRemoteWindowsFormsBlazorWebView();
            serviceCollection.AddScoped<HttpClient>();
            InitializeComponent();

            blazorWebView1.Services = serviceCollection.BuildServiceProvider();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (InitializeRemoteBlazor())
            {
                // Optional: a server whose gRPC endpoint is on another address says so here. Without this the server's own address is used.
                blazorWebView1.GrpcBaseUri = await blazorWebView1.GetGrpcBaseUriAsync(blazorWebView1.ServerUri);

                // Something of the app's own on top of the generated setup: say in the title who is driving.
                blazorWebView1.Connected += (_, connected) =>
                {
                    var user = connected.User.Length > 0 ? $"by user {connected.User} " : "";
                    Text += $" Controlled remotely {user}from ip address {connected.IpAddress}";
                };
            }

            blazorWebView1.RootComponents.Add<App>("#app");
            blazorWebView1.RootComponents.Add<HeadOutlet>("head::after");

            // Last: setting HostPage starts the view, and the server and id must be set before it.
            blazorWebView1.HostPage = @"wwwroot\index.html";
        }
    }
}
