namespace SemanticKernelDemos.WebChat;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChatbotServices(this IServiceCollection services)
    {
        services.AddScoped<ChatbotService>();
        services.AddSingleton<IChatHistoryStore, InMemoryChatbotHistory>();

        return services;
    }
}