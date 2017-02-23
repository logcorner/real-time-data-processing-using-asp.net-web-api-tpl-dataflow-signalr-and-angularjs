using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SignalrAngularClient.Startup))]
namespace SignalrAngularClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
