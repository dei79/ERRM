using ERRM.Models;
using Microsoft.Extensions.AI;

namespace ERRM.Services;

public class AiEvaluationFormulationService(
    IChatClient chatClient,
    IPromptGenerator promptGenerator,
    ILogger<AiEvaluationFormulationService> logger) : IEvaluationFormulationService
{
    public async Task<EvaluationResultViewModel> GenerateAsync(
        EvaluationViewModel evaluation,
        CancellationToken cancellationToken = default)
    {
        var criteriaResults = EvaluationReportMetadata.CreateCriteriaResults(evaluation);
        var averageRating = EvaluationReportMetadata.CalculateAverageRating(criteriaResults);
        var overallLabel = EvaluationReportMetadata.GetOverallLabel(averageRating);
        var prompt = promptGenerator.GenerateEvaluationPrompt(evaluation);

        try
        {
            var response = await chatClient.GetResponseAsync(prompt.Messages, cancellationToken: cancellationToken);

            return new EvaluationResultViewModel
            {
                Evaluation = evaluation,
                OverallLabel = overallLabel,
                AverageRating = averageRating,
                CriteriaResults = criteriaResults,
                RenderedReport = response.Text,
                GenerationSource = "OpenAI ChatClient"
            };
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "AI report generation failed for evaluation {EvaluationId}.", evaluation.Id);

            return new EvaluationResultViewModel
            {
                Evaluation = evaluation,
                OverallLabel = overallLabel,
                AverageRating = averageRating,
                CriteriaResults = criteriaResults,
                GenerationSource = "OpenAI ChatClient",
                ErrorMessage = "The AI report could not be generated. Check the OpenAI configuration and try again."
            };
        }
    }
}
