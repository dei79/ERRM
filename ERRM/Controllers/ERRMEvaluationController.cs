using Microsoft.AspNetCore.Mvc;

namespace ERRM.Controllers
{
    public class ERRMEvaluationController : Controller
    {
        // GET: ERRMEvaluationController
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

    }
}
