using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelDemos.WebChat;

public class InMemoryChatbotHistory : IChatHistoryStore
{
    private readonly Dictionary<string, ChatHistory> _chatHistory = new();

    public ChatHistory GetOrCreateChatHistory(string sessionId)
    {
        if (!_chatHistory.TryGetValue(sessionId, out var chatHistory))
        {
            chatHistory = new ChatHistory();
            _chatHistory.Add(sessionId, chatHistory);
        }

        return chatHistory;
    }
}