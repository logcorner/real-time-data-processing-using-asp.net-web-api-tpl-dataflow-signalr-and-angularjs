using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace ApiSignalrAngularHub.Hubs
{
    [HubName("dashboards")]
    public class DashboardHub : Hub { }
}