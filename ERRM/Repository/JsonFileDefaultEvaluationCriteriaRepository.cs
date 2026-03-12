using System.Text.Json;
using ERRM.Models;

namespace ERRM.Repository;

public class JsonFileDefaultEvaluationCriteriaRepository : IEvaluationCriteriaRepository
{
    private readonly string _criteriaFilePath;

    public JsonFileDefaultEvaluationCriteriaRepository(IHostEnvironment hostEnvironment)
    {
        _criteriaFilePath = Path.Combine(
            hostEnvironment.ContentRootPath,
            "Data",
            "Criterias",
            "DefaultEvaluationCriterias.json");
    }

    public async Task<IReadOnlyList<DefaultEvaluationCriteria>> GetAllAsync()
    {
        if (!File.Exists(_criteriaFilePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_criteriaFilePath);
        var fileModel = JsonSerializer.Deserialize<DefaultEvaluationCriteriaFile>(json);
        return fileModel?.EvaluationCriterias ?? [];
    }
}
