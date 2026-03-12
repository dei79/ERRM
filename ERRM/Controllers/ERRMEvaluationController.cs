using Microsoft.AspNetCore.Mvc;
using ERRM.Models;
using ERRM.Repository;

namespace ERRM.Controllers
{
    public class ERRMEvaluationController(IEvaluationRepository evaluationRepository) : Controller
    {
        // GET: ERRMEvaluationController
        public async Task<ActionResult> Index()
        {
            var evaluations = await evaluationRepository.GetAllAsync();
            return View(evaluations);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> New(EvaluationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await evaluationRepository.SaveAsync(model);
            return RedirectToAction(nameof(Index));
        }

    }
}
