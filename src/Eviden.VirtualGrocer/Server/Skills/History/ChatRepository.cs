using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.Tokenizers;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Text;

namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{

    public class ChatRepository
    {
        private readonly IStorageContext<ChatHistory> _storage;
        public ChatRepository(IStorageContext<ChatHistory> storage) => _storage = storage;

        public async Task<ChatHistory> GetAsync(string chatId)
        {
            ChatHistory? history = await _storage.Get(chatId);

            if (history is null)
            {
                history = new ChatHistory(chatId);
                bool success = await _storage.Create(chatId, history);

                if (!success)
                {
                    history = await _storage.Get(chatId);

                    if (history is null)
                    {
                        throw new ApplicationException("Could not create chat history");
                    }
                }
            }

            return history!;
        }

        public async Task StashAsync(ChatHistory chatHistory) => await _storage.Set(chatHistory.ChatId, chatHistory);
    }
}
