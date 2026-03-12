using System.ComponentModel.DataAnnotations;

namespace ERRM.Models;

public class EvaluationViewModel
{
    public string Id { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    // Issuer information
    [Display(Name = "Company name")]
    public required string CompanyName { get; set; }
    [Display(Name = "Company location")]
    public required string CompanyLocation { get; set; }
    [Display(Name = "Issuing location")]
    public required string IssuingLocation { get; set; }
    [Display(Name = "First signer name")]
    public required string FirstSignerName { get; set; }
    [Display(Name = "Second signer name")]
    public string? SecondSignerName { get; set; }

    // General person information
    [Display(Name = "Salutation")]
    public required string Salutation { get; set; }
    [Display(Name = "Academic title")]
    public string? AcademicTitle { get; set; }
    [Display(Name = "First name")]
    public required string FirstName { get; set; }
    [Display(Name = "Last name")]
    public required string LastName { get; set; }
    [Display(Name = "Date of birth")]
    public DateOnly DateOfBirth { get; set; }

    // Position information
    [Display(Name = "Department")]
    public required string Department { get; set; }
    [Display(Name = "Position")]
    public required string Position { get; set; }
    [Display(Name = "Start date")]
    public DateOnly StartDate { get; set; }

    // Evaluation criteria answers
    public List<EvaluationCriteriaAnswerViewModel> CriteriaAnswers { get; set; } = [];
}
