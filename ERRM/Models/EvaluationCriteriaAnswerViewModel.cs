using System.ComponentModel.DataAnnotations;

namespace ERRM.Models;

public class EvaluationCriteriaAnswerViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string RatingScale { get; set; } = "1-5";

    public bool CommentAllowed { get; set; }

    [Required]
    [Range(1, 5)]
    public int? Rating { get; set; }

    public string? Comment { get; set; }
}
