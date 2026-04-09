using ERRM.Models;

namespace ERRM.Services;

public interface IPromptGenerator
{
    EvaluationPrompt GenerateEvaluationPrompt(EvaluationViewModel evaluation);
}
