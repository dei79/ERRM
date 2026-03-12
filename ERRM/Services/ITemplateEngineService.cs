namespace ERRM.Services;

public interface ITemplateEngineService
{
    string RenderFromFile(string templatePath, IReadOnlyDictionary<string, string> replacements, string fallbackTemplate);
}
