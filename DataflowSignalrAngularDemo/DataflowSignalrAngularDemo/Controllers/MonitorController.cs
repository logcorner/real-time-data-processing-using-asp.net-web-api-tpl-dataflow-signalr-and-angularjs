namespace ApiSignalrAngularHub.Controllers
{
    using ApiSignalrAngularHub.Hubs;
    using DataflowFileService;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    public class MonitorController : SignalRBase<monitorHub>
    {
        [Route("api/Processor")]
        public HttpResponseMessage PostProcessor(Processor item)
        {
            if (item == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // notify all connected clients
            Hub.Clients.All.LoadBalance(item);

            // return the item inside of a 201 response
            return Request.CreateResponse(HttpStatusCode.Created, item);
        }

        [Route("api/FileOrderEntity")]
        public HttpResponseMessage PostProcessor( [FromBody] SalesOrderDetailEntity item)
        {
            if (item == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // notify all connected clients
            Hub.Clients.All.TransformFileToFileOrderEntity(item);

            // return the item inside of a 201 response
            return Request.CreateResponse(HttpStatusCode.Created, item);
        }
    }
}