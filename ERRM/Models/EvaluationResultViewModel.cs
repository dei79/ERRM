namespace ERRM.Models;

public class EvaluationResultViewModel
{
    public required EvaluationViewModel Evaluation { get; set; }

    public required string OverallLabel { get; set; }

    public decimal AverageRating { get; set; }

    public List<GeneratedEvaluationCriterionViewModel> CriteriaResults { get; set; } = [];

    public string RenderedReport { get; set; } = string.Empty;

    public string GenerationSource { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public string TemplatePath { get; set; } = string.Empty;
}
