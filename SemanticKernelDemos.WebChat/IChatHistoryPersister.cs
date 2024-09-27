using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelDemos.WebChat;

public interface IChatHistoryStore
{
    ChatHistory GetOrCreateChatHistory(string sessionId);
}