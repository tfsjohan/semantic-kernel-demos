using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace SemanticKernelDemos.ExtractDetailsFromImageToJson;

#pragma warning disable SKEXP0010

public class OrganizationDetailsExtractor
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AzureOpenAIPromptExecutionSettings _executionSettings = new()
    {
        ResponseFormat = "json_object",
        Temperature = 0.1
    };

    private readonly Kernel _kernel;

    public OrganizationDetailsExtractor(IConfiguration config)
    {
        var endpoint = config["endpoint"];
        var apiKey = config["apikey"];
        var deploymentName = config["deployment"];

        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);

        _kernel = builder.Build();
    }

    public async Task<OrganizationDetails> ExtractDetailsFromImage(
        ReadOnlyMemory<byte> data,
        string? mimeType,
        CancellationToken cancellationToken = default)
    {
        return await ExtractDetailsFromImageContent(
            new ImageContent(data, mimeType),
            cancellationToken);
    }

    public async Task<OrganizationDetails> ExtractDetailsFromImage(
        Uri imageUri,
        CancellationToken cancellationToken = default)
    {
        return await ExtractDetailsFromImageContent(
            new ImageContent(imageUri),
            cancellationToken);
    }

    private async Task<OrganizationDetails> ExtractDetailsFromImageContent(
        ImageContent imageContent,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = ReadPromptFromPromptDirectory(
            "ExtractOrganizationDetailsFromPubAgreementImage.md");
        var history = new ChatHistory(systemPrompt);

        var userContent = new ChatMessageContentItemCollection { imageContent };
        history.AddUserMessage(userContent);

        var chatCompletionService = _kernel.Services.GetRequiredService<IChatCompletionService>();

        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            _executionSettings,
            _kernel,
            cancellationToken
        );

        return ParseResponseContent(result);
    }

    private static OrganizationDetails ParseResponseContent(ChatMessageContent result)
    {
        if (result.Content is null)
        {
            throw new ExtractDetailsException("No content returned from AI model");
        }

        return JsonSerializer.Deserialize<OrganizationDetails>(result.Content, Options)!;
    }

    private string ReadPromptFromPromptDirectory(string filename)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, filename);

        return File.ReadAllText(fullPath);
    }
}