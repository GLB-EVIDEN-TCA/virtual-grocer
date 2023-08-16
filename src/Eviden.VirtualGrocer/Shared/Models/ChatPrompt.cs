namespace Eviden.VirtualGrocer.Shared.Models
{
    public record ChatPrompt(string ChatId)
    {
        public string? Prompt { get; set; }
    }
}
