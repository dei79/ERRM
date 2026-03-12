using ERRM.Models;

namespace ERRM.Repository;

public interface IEvaluationCriteriaRepository
{
    Task<IReadOnlyList<DefaultEvaluationCriteria>> GetAllAsync();
}
