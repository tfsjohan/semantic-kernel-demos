using System.ComponentModel.DataAnnotations;

namespace SemanticKernelDemos.WebChat;

public class ChatRequest
{
    [Required] public string? SessionId { get; set; }

    [Required] public string? Prompt { get; set; }
}