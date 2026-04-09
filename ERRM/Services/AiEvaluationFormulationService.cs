using ERRM.Models;
using Microsoft.Extensions.AI;

namespace ERRM.Services;

public class AiEvaluationFormulationService(
    IChatClient chatClient,
    IPromptGenerator promptGenerator,
    ILogger<AiEvaluationFormulationService> logger) : IEvaluationFormulationService
{
    public IAsyncEnumerable<ReportGenerationUpdate> StreamAsync(
        EvaluationViewModel evaluation,
        CancellationToken cancellationToken = default)
    {
        return StreamCoreAsync(evaluation, cancellationToken);
    }

    // IAsyncEnumerable lets us produce multiple values over time instead of returning one final result.
    // Here each yielded ReportGenerationUpdate becomes one streamed chunk that the controller can forward
    // immediately to the browser while the LLM is still generating the report.
    private async IAsyncEnumerable<ReportGenerationUpdate> StreamCoreAsync(
        EvaluationViewModel evaluation,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prompt = promptGenerator.GenerateEvaluationPrompt(evaluation);
        await using var enumerator = chatClient.GetStreamingResponseAsync(prompt.Messages, cancellationToken: cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        string? errorMessage = null;

        while (true)
        {
            ChatResponseUpdate update;

            try
            {
                if (!await enumerator.MoveNextAsync())
                {
                    break;
                }

                update = enumerator.Current;
            }

            catch (Exception exception)
            {
                logger.LogError(exception, "AI report generation failed for evaluation {EvaluationId}.", evaluation.Id);
                errorMessage = "The AI report could not be generated. Check the OpenAI configuration and try again.";
                break;
            }

            if (!string.IsNullOrEmpty(update.Text))
            {
                // "yield return" sends one chunk immediately to the caller instead of waiting
                // until the whole report is finished. This is what enables visible streaming in the browser.
                yield return new ReportGenerationUpdate
                {
                    Type = "content",
                    Content = update.Text
                };
            }
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            yield return new ReportGenerationUpdate
            {
                Type = "error",
                ErrorMessage = errorMessage
            };

            yield break;
        }

        yield return new ReportGenerationUpdate
        {
            Type = "complete"
        };
    }
}
