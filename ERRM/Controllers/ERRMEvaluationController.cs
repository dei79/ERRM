using Microsoft.AspNetCore.Mvc;
using ERRM.Models;
using ERRM.Repository;

namespace ERRM.Controllers
{
    public class ERRMEvaluationController(
        IEvaluationRepository evaluationRepository,
        IEvaluationCriteriaRepository evaluationCriteriaRepository)
        : Controller
    {
        // GET: ERRMEvaluationController
        public async Task<ActionResult> Index()
        {
            var evaluations = await evaluationRepository.GetAllAsync();
            return View(evaluations);
        }

        public async Task<ActionResult> New()
        {
            var criterias = await evaluationCriteriaRepository.GetAllAsync();

            var model = new EvaluationViewModel
            {
                CompanyName = string.Empty,
                CompanyLocation = string.Empty,
                IssuingLocation = string.Empty,
                FirstSignerName = string.Empty,
                Salutation = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Department = string.Empty,
                Position = string.Empty,
                CriteriaAnswers = criterias.Select(criteria => new EvaluationCriteriaAnswerViewModel
                {
                    Title = criteria.Title,
                    Description = criteria.Description,
                    RatingScale = criteria.RatingScale,
                    CommentAllowed = criteria.CommentAllowed
                }).ToList()
            };

            return View(model);
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
