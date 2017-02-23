using DataflowFileService;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ApiSignalrAngularHub.Controllers
{
    public class HomeController : AsyncController
    {
        private static readonly OrchestratorService orchestratorService = new OrchestratorService();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sender()
        {
            return View();
        }

        public async Task< ActionResult> Receiver()
        {
           await orchestratorService.Execute();
            return View();
        }
    }
}