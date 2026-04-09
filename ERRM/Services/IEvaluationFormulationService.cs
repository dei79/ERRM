using ERRM.Models;

namespace ERRM.Services;

public interface IEvaluationFormulationService
{
    Task<EvaluationResultViewModel> GenerateAsync(
        EvaluationViewModel evaluation,
        CancellationToken cancellationToken = default);
}
