using ERRM.Models;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ClientModel;

namespace ERRM.Services;

public static class OpenAIServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AIOptions>(configuration.GetSection(AIOptions.SectionName));

        var aiOptions = configuration.GetSection(AIOptions.SectionName).Get<AIOptions>() ?? new AIOptions();
        var configuredModel = configuration[$"{AIOptions.SectionName}:{nameof(AIOptions.Model)}"];
        var configuredApiKey = configuration[$"{AIOptions.SectionName}:{nameof(AIOptions.ApiKey)}"];
        var openAIModel = string.IsNullOrWhiteSpace(configuredModel)
            ? aiOptions.Model
            : configuredModel;
        var openAIApiKey = string.IsNullOrWhiteSpace(configuredApiKey)
            ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            : configuredApiKey;

        if (string.IsNullOrWhiteSpace(openAIModel))
        {
            throw new InvalidOperationException("AI model configuration is missing. Set AI:Model in configuration.");
        }

        if (string.IsNullOrWhiteSpace(openAIApiKey))
        {
            throw new InvalidOperationException("OpenAI API key configuration is missing. Set AI:ApiKey or OPENAI_API_KEY.");
        }

        // To use OpenAI locally, set AI__Model plus either AI__ApiKey or OPENAI_API_KEY before starting the app.
        services.AddSingleton<IChatClient>(_ =>
            new ChatClient(openAIModel, new ApiKeyCredential(openAIApiKey)).AsIChatClient());

        return services;
    }
}
