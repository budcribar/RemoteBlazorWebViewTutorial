using System;
using System.Linq;

namespace RemoteBlazorWebViewTutorial.Shared
{
    public class RunString
    {
        public Uri? ServerUri { get; set; }
        public Guid Id { get; set; } = Guid.Empty;
        public bool IsRestarting { get; set; } = false;

        public RunString()
        {
            IsRestarting = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-r")) != null;

            try
            {
                // -u=https://localhost:443
                var u = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-u"));
                if (u != null)
                    ServerUri = new Uri(u.Split("=")[1]);
            }
            catch (Exception) { }
            try
            {
                // -i=9BFD9D43-0289-4A80-92D8-6E617729DA12
                var i = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-i"));
                if (i != null)
                    Id = Guid.Parse(i.Split("=")[1]);
            }
            catch (Exception) { Id = Guid.NewGuid(); }
        }
    }
}
