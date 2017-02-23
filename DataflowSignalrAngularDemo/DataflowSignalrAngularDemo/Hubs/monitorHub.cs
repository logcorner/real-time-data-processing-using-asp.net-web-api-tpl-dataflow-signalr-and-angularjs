using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace ApiSignalrAngularHub.Hubs
{
    [HubName("monitors")]
    public class monitorHub : Hub { }
}