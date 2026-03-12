using System.Text.Json;
using ERRM.Models;

namespace ERRM.Repository;

public class JsonFileEvaluationRepository : IEvaluationRepository
{
    private readonly string _storagePath;

    public JsonFileEvaluationRepository(IHostEnvironment hostEnvironment)
    {
        _storagePath = Path.Combine(hostEnvironment.ContentRootPath, "Data", "Evaluations");
    }

    public async Task<IReadOnlyList<EvaluationViewModel>> GetAllAsync()
    {
        if (!Directory.Exists(_storagePath))
        {
            return [];
        }

        var models = new List<EvaluationViewModel>();
        var files = Directory.GetFiles(_storagePath, "*.json")
            .OrderByDescending(file => file);

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var model = JsonSerializer.Deserialize<EvaluationViewModel>(json);
                if (model is not null)
                {
                    models.Add(model);
                }
            }
            catch (JsonException)
            {
                // Skip invalid JSON files to keep the list view resilient.
            }
        }

        return models
            .OrderByDescending(model => model.CreatedAtUtc)
            .ThenByDescending(model => model.Id)
            .ToList();
    }

    public async Task<EvaluationViewModel?> GetByIdAsync(string id)
    {
        if (!Directory.Exists(_storagePath))
        {
            return null;
        }

        var files = Directory.GetFiles(_storagePath, "*.json")
            .OrderByDescending(file => file);

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var model = JsonSerializer.Deserialize<EvaluationViewModel>(json);
                if (model?.Id == id)
                {
                    return model;
                }
            }
            catch (JsonException)
            {
                // Skip invalid JSON files to keep the lookup resilient.
            }
        }

        return null;
    }

    public async Task SaveAsync(EvaluationViewModel model)
    {
        Directory.CreateDirectory(_storagePath);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}.json";
        var fullPath = Path.Combine(_storagePath, fileName);

        var json = JsonSerializer.Serialize(model, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(fullPath, json);
    }
}
