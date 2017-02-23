using DataflowFileService;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(ApiSignalrAngularHub.Startup))]

namespace ApiSignalrAngularHub
{
    public class Startup
    {
        private static readonly OrderService orchestratorService = new OrderService();

        public void Configuration(IAppBuilder app)
        {
            app.Map("/signalr", map =>
           {
               map.UseCors(CorsOptions.AllowAll);
               var hubConfiguration = new HubConfiguration { };
               hubConfiguration.EnableJSONP = true;
               map.RunSignalR(hubConfiguration);
           });

            Task.Run(() => orchestratorService.Execute());
        }
    }
}