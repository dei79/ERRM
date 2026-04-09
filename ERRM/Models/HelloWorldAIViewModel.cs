namespace ERRM.Models;

public class HelloWorldAIViewModel
{
    public string SystemPrompt { get; set; } =
        "You are a concise assistant. Answer clearly and briefly.";

    public string Prompt { get; set; } = "Say hello world in one short sentence.";

    public string Response { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
