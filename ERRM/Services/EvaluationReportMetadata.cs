using System.Globalization;
using System.Text;
using ERRM.Models;

namespace ERRM.Services;

public static class EvaluationReportMetadata
{
    public static EvaluationResultViewModel CreateResultSkeleton(
        EvaluationViewModel evaluation,
        string generationSource,
        string templatePath = "")
    {
        var criteriaResults = CreateCriteriaResults(evaluation);
        var averageRating = CalculateAverageRating(criteriaResults);
        var overallLabel = GetOverallLabel(averageRating);

        return new EvaluationResultViewModel
        {
            Evaluation = evaluation,
            OverallLabel = overallLabel,
            AverageRating = averageRating,
            CriteriaResults = criteriaResults,
            GenerationSource = generationSource,
            TemplatePath = templatePath
        };
    }

    public static List<GeneratedEvaluationCriterionViewModel> CreateCriteriaResults(EvaluationViewModel evaluation)
    {
        return evaluation.CriteriaAnswers
            .Where(answer => answer.Rating.HasValue)
            .Select(answer => new GeneratedEvaluationCriterionViewModel
            {
                Title = answer.Title,
                Rating = answer.Rating!.Value,
                Formulation = ResolveFormulation(answer),
                Comment = string.IsNullOrWhiteSpace(answer.Comment) ? null : answer.Comment.Trim()
            })
            .ToList();
    }

    public static decimal CalculateAverageRating(IEnumerable<GeneratedEvaluationCriterionViewModel> criteriaResults)
    {
        var ratedCriteria = criteriaResults.ToList();

        return ratedCriteria.Count == 0
            ? 0m
            : Math.Round(ratedCriteria.Average(item => (decimal)item.Rating), 2, MidpointRounding.AwayFromZero);
    }

    public static string ResolveFormulation(EvaluationCriteriaAnswerViewModel answer)
    {
        var ratingKey = answer.Rating?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        if (answer.RatingScaleFormulations.TryGetValue(ratingKey, out var formulation) &&
            !string.IsNullOrWhiteSpace(formulation))
        {
            return formulation.Trim();
        }

        return $"Was rated {ratingKey} on the {answer.RatingScale} scale.";
    }

    public static string GetOverallLabel(decimal averageRating)
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

    public static string BuildFullName(EvaluationViewModel evaluation)
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

    public static string BuildCriteriaSummary(IEnumerable<GeneratedEvaluationCriterionViewModel> criteriaResults)
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

    public static string BuildSignerBlock(EvaluationViewModel evaluation)
    {
        return string.IsNullOrWhiteSpace(evaluation.SecondSignerName)
            ? evaluation.FirstSignerName
            : $"{evaluation.FirstSignerName}\n{evaluation.SecondSignerName}";
    }
}
