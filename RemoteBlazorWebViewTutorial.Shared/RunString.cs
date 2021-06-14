using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteBlazorWebViewTutorial.Shared
{
    public class RunString
    {
        public Uri? ServerUri { get; set; }
        public Guid Id { get; set; } //= Guid.Parse("d8d19338-3d66-4942-912b-5b3103efa177");
        public bool IsRestarting { get; set; } = false;

        public RunString()
        {
            IsRestarting = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-r")) != null;

            // -u=https://localhost:443 -i=9BFD9D43-0289-4A80-92D8-6E617729DA12
            try
            {
                var u = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-u"));
                if (u != null)
                    ServerUri = new Uri(u.Split("=")[1]);
            }
            catch (Exception) { }
            try
            {
                var i = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-i"));
                if (i != null)
                    Id = Guid.Parse(i.Split("=")[1]);
            }
            catch (Exception) { Id = Guid.NewGuid(); }
        }
    }
}
