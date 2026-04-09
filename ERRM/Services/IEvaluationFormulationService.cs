using ERRM.Models;

namespace ERRM.Services;

public interface IEvaluationFormulationService
{
    IAsyncEnumerable<ReportGenerationUpdate> StreamAsync(
        EvaluationViewModel evaluation,
        CancellationToken cancellationToken = default);
}
