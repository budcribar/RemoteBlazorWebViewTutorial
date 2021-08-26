using System;
using System.Linq;

namespace RemoteBlazorWebViewTutorial.Shared
{
    public class AppSettings
    {
        public Uri? ServerUrl { get; set; }
        public Guid Id { get; set; } = Guid.Empty;
        public bool IsRestarting { get; set; } = false;
    }
}
