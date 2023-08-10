using Microsoft.SemanticKernel.Connectors.AI.OpenAI.Tokenizers;

namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
    public class TokenCounter : ITokenCounter
    {
        public int CountTokens(string message) => GPT3Tokenizer.Encode(message).Count;
    }
}
