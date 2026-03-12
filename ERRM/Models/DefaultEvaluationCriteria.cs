using System.Text.Json.Serialization;

namespace ERRM.Models;

public class DefaultEvaluationCriteria
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("ratingScale")]
    public string RatingScale { get; set; } = string.Empty;

    [JsonPropertyName("ratingScaleFormulations")]
    public Dictionary<string, string> RatingScaleFormulations { get; set; } = [];

    [JsonPropertyName("commentAllowed")]
    public bool CommentAllowed { get; set; }
}
