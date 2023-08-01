using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;

namespace InventoryPoc.Chat
{
    internal class MerchBot : IChatCompletion
    {
        private readonly IKernel _kernel;
        public MerchBot(IKernel kernel)
        {
            _kernel = kernel;
        }

        public ChatHistory CreateNewChat(string instructions = "")
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateMessageAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<string> GenerateMessageStreamAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
