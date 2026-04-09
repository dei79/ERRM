using System.Globalization;
using System.Text;
using ERRM.Models;

namespace ERRM.Services;

public class RuleBasedEvaluationFormulationService : IEvaluationFormulationService
{
    private readonly string _templatePath;
    private readonly ITemplateEngineService _templateEngineService;

    public RuleBasedEvaluationFormulationService(
        IHostEnvironment hostEnvironment,
        ITemplateEngineService templateEngineService)
    {
        _templateEngineService = templateEngineService;
        _templatePath = Path.Combine(
            hostEnvironment.ContentRootPath,
            "Data",
            "Templates",
            "EvaluationReportTemplate.txt");
    }

    public async IAsyncEnumerable<ReportGenerationUpdate> StreamAsync(
        EvaluationViewModel evaluation,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var result = EvaluationReportMetadata.CreateResultSkeleton(evaluation, "Rule-based template", _templatePath);
        result.RenderedReport = BuildRenderedReport(
            evaluation,
            result.CriteriaResults,
            result.AverageRating,
            result.OverallLabel);

        foreach (var line in result.RenderedReport.Split(Environment.NewLine))
        {
            if (!string.IsNullOrEmpty(line))
            {
                // The rule-based version also emits the report piece by piece so both implementations
                // share the same streaming contract, even though this text is already available upfront.
                yield return new ReportGenerationUpdate
                {
                    Type = "content",
                    Content = $"{line}{Environment.NewLine}"
                };
            }
        }

        yield return new ReportGenerationUpdate
        {
            Type = "complete"
        };
    }

    private string BuildRenderedReport(
        EvaluationViewModel evaluation,
        IEnumerable<GeneratedEvaluationCriterionViewModel> criteriaResults,
        decimal averageRating,
        string overallLabel)
    {
        var fullName = EvaluationReportMetadata.BuildFullName(evaluation);
        var criteriaSummary = EvaluationReportMetadata.BuildCriteriaSummary(criteriaResults);
        var replacements = new Dictionary<string, string>
        {
            ["{{CompanyName}}"] = evaluation.CompanyName,
            ["{{CompanyLocation}}"] = evaluation.CompanyLocation,
            ["{{IssuingLocation}}"] = evaluation.IssuingLocation,
            ["{{CurrentDate}}"] = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["{{FirstSignerName}}"] = evaluation.FirstSignerName,
            ["{{SecondSignerName}}"] = evaluation.SecondSignerName ?? string.Empty,
            ["{{SignerBlock}}"] = EvaluationReportMetadata.BuildSignerBlock(evaluation),
            ["{{Salutation}}"] = evaluation.Salutation,
            ["{{AcademicTitle}}"] = evaluation.AcademicTitle ?? string.Empty,
            ["{{FirstName}}"] = evaluation.FirstName,
            ["{{LastName}}"] = evaluation.LastName,
            ["{{FullName}}"] = fullName,
            ["{{DateOfBirth}}"] = evaluation.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["{{Department}}"] = evaluation.Department,
            ["{{Position}}"] = evaluation.Position,
            ["{{StartDate}}"] = evaluation.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["{{AverageRating}}"] = averageRating.ToString("0.00", CultureInfo.InvariantCulture),
            ["{{OverallLabel}}"] = overallLabel,
            ["{{OverallLabelLower}}"] = overallLabel.ToLowerInvariant(),
            ["{{CriteriaSummary}}"] = criteriaSummary
        };

        return _templateEngineService.RenderFromFile(_templatePath, replacements, GetFallbackTemplate());
    }

    private static string GetFallbackTemplate()
    {
        return """
{{CompanyName}}
{{CompanyLocation}}

{{IssuingLocation}}, {{CurrentDate}}

Subject: Performance evaluation for {{FullName}}

Dear {{Salutation}} {{LastName}},

This report documents the performance evaluation for {{FullName}}, born on {{DateOfBirth}}, who has been employed as {{Position}} in the {{Department}} department since {{StartDate}}.

Based on the completed evaluation, the overall performance is assessed as {{OverallLabelLower}} with an average rating of {{AverageRating}}.

Criteria-based assessment:
{{CriteriaSummary}}

Sincerely,

{{SignerBlock}}
""";
    }
}
