using System.Globalization;
using System.Text;
using ERRM.Models;
using Microsoft.Extensions.AI;

namespace ERRM.Services;

public class EvaluationPromptGenerator : IPromptGenerator
{
    public EvaluationPrompt GenerateEvaluationPrompt(EvaluationViewModel evaluation)
    {
        var criteriaResults = EvaluationReportMetadata.CreateCriteriaResults(evaluation);
        var averageRating = EvaluationReportMetadata.CalculateAverageRating(criteriaResults);
        var overallLabel = EvaluationReportMetadata.GetOverallLabel(averageRating);
        var promptText = BuildPromptText(evaluation, criteriaResults, averageRating, overallLabel);

        return new EvaluationPrompt
        {
            Messages =
            [
                new ChatMessage(ChatRole.System,
                    "You write professional employee evaluation reports. Stay factual, align with the provided ratings, and do not invent missing details."),
                new ChatMessage(ChatRole.User, promptText)
            ],
            PromptPreview = promptText
        };
    }

    private static string BuildPromptText(
        EvaluationViewModel evaluation,
        IEnumerable<GeneratedEvaluationCriterionViewModel> criteriaResults,
        decimal averageRating,
        string overallLabel)
    {
        var builder = new StringBuilder();
        var fullName = EvaluationReportMetadata.BuildFullName(evaluation);

        builder.AppendLine("Create a formal employee evaluation report using the data below.");
        builder.AppendLine("Return report text only, without markdown or bullet points in the final answer.");
        builder.AppendLine();
        builder.AppendLine("Expected report structure:");
        builder.AppendLine("- Company header with company name and company location");
        builder.AppendLine("- Issuing location and current date");
        builder.AppendLine("- Subject line for the employee evaluation");
        builder.AppendLine("- Greeting to the employee");
        builder.AppendLine("- Introduction summarizing person, role, department, and employment start");
        builder.AppendLine("- Overall assessment using the provided average rating and overall label");
        builder.AppendLine("- Criteria-based assessment using the provided formulations and comments");
        builder.AppendLine("- Personnel file sentence for the company");
        builder.AppendLine("- Professional closing with signer block");
        builder.AppendLine();
        builder.AppendLine("Company and signer data:");
        builder.AppendLine($"- Company name: {evaluation.CompanyName}");
        builder.AppendLine($"- Company location: {evaluation.CompanyLocation}");
        builder.AppendLine($"- Issuing location: {evaluation.IssuingLocation}");
        builder.AppendLine($"- First signer: {evaluation.FirstSignerName}");
        builder.AppendLine($"- Second signer: {evaluation.SecondSignerName ?? "none"}");
        builder.AppendLine();
        builder.AppendLine("Employee data:");
        builder.AppendLine($"- Salutation: {evaluation.Salutation}");
        builder.AppendLine($"- Academic title: {evaluation.AcademicTitle ?? "none"}");
        builder.AppendLine($"- First name: {evaluation.FirstName}");
        builder.AppendLine($"- Last name: {evaluation.LastName}");
        builder.AppendLine($"- Full name: {fullName}");
        builder.AppendLine($"- Date of birth: {evaluation.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        builder.AppendLine($"- Department: {evaluation.Department}");
        builder.AppendLine($"- Position: {evaluation.Position}");
        builder.AppendLine($"- Start date: {evaluation.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        builder.AppendLine();
        builder.AppendLine("Deterministic evaluation metadata:");
        builder.AppendLine($"- Average rating: {averageRating.ToString("0.00", CultureInfo.InvariantCulture)}");
        builder.AppendLine($"- Overall label: {overallLabel}");
        builder.AppendLine();
        builder.AppendLine("Criteria results:");

        foreach (var criterion in criteriaResults)
        {
            builder.AppendLine($"- Title: {criterion.Title}");
            builder.AppendLine($"  Rating: {criterion.Rating}");
            builder.AppendLine($"  Formulation: {criterion.Formulation}");

            if (!string.IsNullOrWhiteSpace(criterion.Comment))
            {
                builder.AppendLine($"  Comment: {criterion.Comment}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Use the rating formulations verbatim where they fit naturally, keep the tone professional, and ensure the report remains internally consistent.");

        return builder.ToString().TrimEnd();
    }
}
