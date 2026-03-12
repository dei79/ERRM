using ERRM.Models;

namespace ERRM.Repository;

public interface IEvaluationRepository
{
    Task<IReadOnlyList<EvaluationViewModel>> GetAllAsync();
    Task SaveAsync(EvaluationViewModel model);
}
