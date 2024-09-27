using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace SemanticKernelDemos.WebChat;

#pragma warning disable SKEXP0010

public class ChatbotService
{
    private readonly IChatHistoryStore _chatHistoryStore;

    private readonly AzureOpenAIPromptExecutionSettings _executionSettings = new()
    {
        Temperature = 0.7
    };

    private readonly Kernel _kernel;

    public ChatbotService(IConfiguration config, IChatHistoryStore chatHistoryStore)
    {
        _chatHistoryStore = chatHistoryStore;

        var endpoint = config["endpoint"];
        var apiKey = config["apikey"];
        var deploymentName = config["deployment"];

        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);

        _kernel = builder.Build();
    }

    public async IAsyncEnumerable<string> GenerateResponse(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.Prompt))
        {
            throw new InvalidOperationException("Prompt must be provided.");
        }

        if (string.IsNullOrEmpty(request.SessionId))
        {
            throw new InvalidOperationException("SessionId must be provided.");
        }

        var history = GetChatHistoryWithSystemPrompt(request.SessionId);

        history.AddUserMessage(request.Prompt);

        var service = _kernel.GetRequiredService<IChatCompletionService>();
        var results = service.GetStreamingChatMessageContentsAsync(
            history,
            _executionSettings,
            _kernel,
            cancellationToken);
        var lastResponse = string.Empty;
        await foreach (var response in results)
        {
            if (response.Content == null)
            {
                continue;
            }

            yield return response.Content;
            lastResponse += response.Content;
        }

        history.AddAssistantMessage(lastResponse);
    }

    private ChatHistory GetChatHistoryWithSystemPrompt(string sessionId)
    {
        var history = _chatHistoryStore.GetOrCreateChatHistory(sessionId);
        var systemPrompt = ReadPromptFromPromptDirectory(
            "Prompt.md");

        if (history.Count == 0)
        {
            history.AddSystemMessage(systemPrompt);
        }

        return history;
    }

    private string ReadPromptFromPromptDirectory(string filename)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, filename);

        return File.ReadAllText(fullPath);
    }
}