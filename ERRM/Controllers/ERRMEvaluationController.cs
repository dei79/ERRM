using Microsoft.AspNetCore.Mvc;
using ERRM.Models;
using ERRM.Repository;

namespace ERRM.Controllers
{
    public class ERRMEvaluationController(
        IEvaluationRepository evaluationRepository)
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
                CriteriaAnswers = []
            };

            model.CriteriaAnswers.Add(new EvaluationCriteriaAnswerViewModel() 
            { 
                Title = "Work quality", 
                Description = "Evaluate the quality of work produced by the employee.", 
                RatingScale = "1-5", 
                CommentAllowed = true 
            });
            
            model.CriteriaAnswers.Add(new EvaluationCriteriaAnswerViewModel() 
            { 
                Title = "Communication skills", 
                Description = "Assess the employee's ability to communicate effectively.", 
                RatingScale = "1-5", 
                CommentAllowed = true 
            });
            
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
