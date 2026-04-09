using Microsoft.Extensions.AI;

namespace ERRM.Models;

public class EvaluationPrompt
{
    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    public string PromptPreview { get; init; } = string.Empty;
}
