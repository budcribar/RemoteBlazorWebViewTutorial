using System.Net.Http;

using BlazorWebView;
using RemoteBlazorWebViewTutorial.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<HttpClient>();
        }

        /// <summary>
        /// Configure the app.
        /// </summary>
        /// <param name="app">The application builder for apps.</param>
        public void Configure(ApplicationBuilder app)
        {
            app.AddComponent<Shared.App>("app");
        }
    }
}