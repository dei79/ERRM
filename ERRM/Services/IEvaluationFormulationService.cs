using ERRM.Models;

namespace ERRM.Services;

public interface IEvaluationFormulationService
{
    EvaluationResultViewModel Generate(EvaluationViewModel evaluation);
}
