using ERRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace ERRM.Controllers;

[Authorize]
public class AIController(
    IChatClient chatClient,
    ILogger<AIController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new HelloWorldAIViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(HelloWorldAIViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Prompt))
        {
            model.ErrorMessage = "Please enter a prompt before sending the request.";
            return View(model);
        }

        try
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(model.SystemPrompt))
            {
                messages.Add(new ChatMessage(ChatRole.System, model.SystemPrompt.Trim()));
            }

            messages.Add(new ChatMessage(ChatRole.User, model.Prompt.Trim()));

            var response = await chatClient.GetResponseAsync(
                messages,
                cancellationToken: cancellationToken);

            model.Response = response.Text;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Hello world AI request failed.");
            model.ErrorMessage = "The AI request failed. Check the OpenAI configuration and try again.";
        }

        return View(model);
    }
}
