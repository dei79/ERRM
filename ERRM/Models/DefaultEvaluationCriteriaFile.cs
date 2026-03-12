using System.Text.Json.Serialization;

namespace ERRM.Models;

public class DefaultEvaluationCriteriaFile
{
    [JsonPropertyName("evaluationCriterias")]
    public List<DefaultEvaluationCriteria> EvaluationCriterias { get; set; } = [];
}
