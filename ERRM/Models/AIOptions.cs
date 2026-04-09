namespace ERRM.Models;

public class AIOptions
{
    public const string SectionName = "AI";

    public string Model { get; set; } = "gpt-4.1-mini";

    public string ApiKey { get; set; } = string.Empty;
}
