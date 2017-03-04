namespace ApiSignalrAngularHub.Controllers.Api
{
    using ApiSignalrAngularHub.Hubs;
    using DataflowFileService;
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class DashboardController : SignalRBase<DashboardHub>
    {
        [Route("api/Processor")]
        [HttpPost]
        public async Task<IHttpActionResult> PostProcessor(Processor item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest();
                }
                await Task.Run(() => Hub.Clients.All.LoadBalance(item));
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("api/SalesOrderDetailEntity")]
        [HttpPost]
        public async Task<IHttpActionResult> PostSalesOrderDetailEntity(
            [FromBody] SalesOrderDetailEntity item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest();
                }

                await Task.Run(() => Hub.Clients.All.TransformSalesOrderDetailEntity(item));
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}