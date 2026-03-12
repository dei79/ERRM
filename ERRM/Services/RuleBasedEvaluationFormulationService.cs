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

    public EvaluationResultViewModel Generate(EvaluationViewModel evaluation)
    {
        var ratedCriteria = evaluation.CriteriaAnswers
            .Where(answer => answer.Rating.HasValue)
            .Select(answer => new GeneratedEvaluationCriterionViewModel
            {
                Title = answer.Title,
                Rating = answer.Rating!.Value,
                Formulation = ResolveFormulation(answer),
                Comment = string.IsNullOrWhiteSpace(answer.Comment) ? null : answer.Comment.Trim()
            })
            .ToList();

        var averageRating = ratedCriteria.Count == 0
            ? 0m
            : Math.Round(ratedCriteria.Average(item => (decimal)item.Rating), 2, MidpointRounding.AwayFromZero);

        var overallLabel = GetOverallLabel(averageRating);

        return new EvaluationResultViewModel
        {
            Evaluation = evaluation,
            OverallLabel = overallLabel,
            AverageRating = averageRating,
            CriteriaResults = ratedCriteria,
            RenderedReport = BuildRenderedReport(evaluation, ratedCriteria, averageRating, overallLabel),
            TemplatePath = _templatePath
        };
    }

    private static string ResolveFormulation(EvaluationCriteriaAnswerViewModel answer)
    {
        var ratingKey = answer.Rating?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        if (answer.RatingScaleFormulations.TryGetValue(ratingKey, out var formulation) &&
            !string.IsNullOrWhiteSpace(formulation))
        {
            return formulation.Trim();
        }

        return $"Was rated {ratingKey} on the {answer.RatingScale} scale.";
    }

    private string BuildRenderedReport(
        EvaluationViewModel evaluation,
        IEnumerable<GeneratedEvaluationCriterionViewModel> criteriaResults,
        decimal averageRating,
        string overallLabel)
    {
        var fullName = BuildFullName(evaluation);
        var criteriaSummary = BuildCriteriaSummary(criteriaResults);
        var replacements = new Dictionary<string, string>
        {
            ["{{CompanyName}}"] = evaluation.CompanyName,
            ["{{CompanyLocation}}"] = evaluation.CompanyLocation,
            ["{{IssuingLocation}}"] = evaluation.IssuingLocation,
            ["{{CurrentDate}}"] = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["{{FirstSignerName}}"] = evaluation.FirstSignerName,
            ["{{SecondSignerName}}"] = evaluation.SecondSignerName ?? string.Empty,
            ["{{SignerBlock}}"] = BuildSignerBlock(evaluation),
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

    private static string GetOverallLabel(decimal averageRating)
    {
        if (averageRating >= 4.5m)
        {
            return "Outstanding";
        }

        if (averageRating >= 3.5m)
        {
            return "Strong";
        }

        if (averageRating >= 2.5m)
        {
            return "Solid";
        }

        if (averageRating >= 1.5m)
        {
            return "Developing";
        }

        return "Critical";
    }

    private static string BuildFullName(EvaluationViewModel evaluation)
    {
        return string.Join(
            " ",
            new[]
            {
                evaluation.Salutation,
                evaluation.AcademicTitle,
                evaluation.FirstName,
                evaluation.LastName
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string BuildCriteriaSummary(IEnumerable<GeneratedEvaluationCriterionViewModel> criteriaResults)
    {
        var builder = new StringBuilder();

        foreach (var criterion in criteriaResults)
        {
            builder.Append($"{criterion.Title}: {criterion.Formulation}");

            if (!string.IsNullOrWhiteSpace(criterion.Comment))
            {
                builder.Append($" Comment: {criterion.Comment}");
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildSignerBlock(EvaluationViewModel evaluation)
    {
        return string.IsNullOrWhiteSpace(evaluation.SecondSignerName)
            ? evaluation.FirstSignerName
            : $"{evaluation.FirstSignerName}\n{evaluation.SecondSignerName}";
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
