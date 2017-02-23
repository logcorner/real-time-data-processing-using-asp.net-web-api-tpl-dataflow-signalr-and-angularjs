using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ApiSignalrAngularHub.Startup))]

namespace ApiSignalrAngularHub
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}