namespace ERRM.Models;

public class ReportGenerationUpdate
{
    public string Type { get; set; } = "content";

    public string Content { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
