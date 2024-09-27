namespace SemanticKernelDemos.WebChat.Models;

public class ChatMessage
{
    public enum ParticipantRole
    {
        User,
        Assistant
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public required ParticipantRole Role { get; set; }

    public required string Text { get; set; }
}