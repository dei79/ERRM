using ERRM.Models;

namespace ERRM.Repository;

public interface IEvaluationRepository
{
    Task<IReadOnlyList<EvaluationViewModel>> GetAllAsync();
    Task<EvaluationViewModel?> GetByIdAsync(string id);
    Task SaveAsync(EvaluationViewModel model);
}
