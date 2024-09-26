﻿using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

#region Setup

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0050

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

#endregion

builder.Plugins.AddFromType<TimePlugin>("TimePlugin");

var bingConnector = new BingConnector(config["bing_key"]!);
var bingPlugin = new WebSearchEnginePlugin(bingConnector);
builder.Plugins.AddFromObject(bingPlugin, "BingPlugin");

var kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory(systemPrompt);

#region Chat loop

while (true)
{
    // Get user input
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.Write("User > ");
    Console.ForegroundColor = ConsoleColor.Blue;
    history.AddUserMessage(Console.ReadLine()!);

    OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
    {
        MaxTokens = 200,
        Temperature = 0.9,
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Get the response from the AI
    var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAiPromptExecutionSettings,
        kernel: kernel);


    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.Write("\nAssistant > ");
    Console.ForegroundColor = ConsoleColor.Green;

    var combinedResponse = string.Empty;
    await foreach (var message in response)
    {
        //Write the response to the console
        Console.Write(message);
        combinedResponse += message;
    }

    Console.WriteLine();

    // Add the message from the agent to the chat history
    history.AddAssistantMessage(combinedResponse);
}

#endregion