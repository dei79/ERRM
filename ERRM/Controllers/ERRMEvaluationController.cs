using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ERRM.Models;
using ERRM.Repository;
using ERRM.Services;
using Microsoft.AspNetCore.Authorization;

namespace ERRM.Controllers
{
    [Authorize]
    public class ERRMEvaluationController(
        IEvaluationRepository evaluationRepository,
        IEvaluationCriteriaRepository evaluationCriteriaRepository,
        IEvaluationFormulationService evaluationFormulationService)
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
            };

            await PopulateCriteriaMetadataAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> New(EvaluationViewModel model)
        {
            await PopulateCriteriaMetadataAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Id = Guid.NewGuid().ToString("N");
            model.CreatedAtUtc = DateTime.UtcNow;

            await evaluationRepository.SaveAsync(model);
            return RedirectToAction(nameof(Result), new { id = model.Id });
        }

        public async Task<ActionResult> Result(string id, CancellationToken cancellationToken)
        {
            var evaluation = await evaluationRepository.GetByIdAsync(id);
            if (evaluation is null)
            {
                return NotFound();
            }

            var result = EvaluationReportMetadata.CreateResultSkeleton(evaluation, "OpenAI ChatClient");
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task StreamReport(string id, CancellationToken cancellationToken)
        {
            var evaluation = await evaluationRepository.GetByIdAsync(id);
            if (evaluation is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "application/x-ndjson";

            await foreach (var update in evaluationFormulationService.StreamAsync(evaluation, cancellationToken))
            {
                await JsonSerializer.SerializeAsync(Response.Body, update, cancellationToken: cancellationToken);
                await Response.WriteAsync("\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        private async Task PopulateCriteriaMetadataAsync(EvaluationViewModel model)
        {
            var criterias = await evaluationCriteriaRepository.GetAllAsync();
            if (model.CriteriaAnswers.Count == 0)
            {
                model.CriteriaAnswers = criterias.Select(criteria => new EvaluationCriteriaAnswerViewModel
                {
                    Title = criteria.Title,
                    Description = criteria.Description,
                    RatingScale = criteria.RatingScale,
                    RatingScaleFormulations = new Dictionary<string, string>(criteria.RatingScaleFormulations),
                    CommentAllowed = criteria.CommentAllowed
                }).ToList();
                return;
            }

            for (var index = 0; index < model.CriteriaAnswers.Count && index < criterias.Count; index++)
            {
                var answer = model.CriteriaAnswers[index];
                var criteria = criterias[index];

                answer.Title = criteria.Title;
                answer.Description = criteria.Description;
                answer.RatingScale = criteria.RatingScale;
                answer.RatingScaleFormulations = new Dictionary<string, string>(criteria.RatingScaleFormulations);
                answer.CommentAllowed = criteria.CommentAllowed;
            }
        }
    }
}
