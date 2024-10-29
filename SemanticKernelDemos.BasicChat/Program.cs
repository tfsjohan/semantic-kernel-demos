using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

var systemPrompt = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Prompt.md"));

var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        config["deployment"]!,
        config["endpoint"]!,
        config["apikey"]!);

var kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory(systemPrompt);

OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    MaxTokens = 1000, // How long the response can be
    Temperature = 0.9, // How creative the AI can be
};

Console.Clear();

while (true)
{
    // Get user input
    var userInput = AskUserForInput();
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAiPromptExecutionSettings,
        kernel: kernel);


    OutputAssistantName();

    // Stream the response from the AI to the console
    var combinedResponse = string.Empty;
    await foreach (var message in response)
    {
        WriteChunkToConsole(message);
        combinedResponse += message;
    }

    // Add the message from the agent to the chat history
    history.AddAssistantMessage(combinedResponse);
}

string AskUserForInput()
{
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.Write("\n\nUser > ");
    Console.ForegroundColor = ConsoleColor.Blue;

    return Console.ReadLine()!;
}

void OutputAssistantName()
{
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.Write("\nAssistant > ");
    Console.ForegroundColor = ConsoleColor.Green;
}

void WriteChunkToConsole(StreamingChatMessageContent? chunk)
{
    Console.Write(chunk);
}