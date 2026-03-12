namespace ERRM.Services;

public class SimpleTemplateEngineService : ITemplateEngineService
{
    public string RenderFromFile(
        string templatePath,
        IReadOnlyDictionary<string, string> replacements,
        string fallbackTemplate)
    {
        var template = File.Exists(templatePath)
            ? File.ReadAllText(templatePath)
            : fallbackTemplate;

        foreach (var replacement in replacements)
        {
            template = template.Replace(replacement.Key, replacement.Value);
        }

        return template.TrimEnd();
    }
}
