using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.Tokenizers;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Text;

namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
    public interface IStorageContext<T>
    {
        Task<T?> Get(string key);

        Task<bool> Create(string key, T value);

        Task Set(string key, T value);

        Task<bool> Delete(string key);
    }

    public class MemoryStorageContext<T> : IStorageContext<T>
    {
        private readonly ConcurrentDictionary<string, T> _storage = new();
        public Task<bool> Create(string key, T value) =>
            Task.FromResult(_storage.TryAdd(key, value));

        public Task<bool> Delete(string key) =>
            Task.FromResult(_storage.TryRemove(key, out _));

        public Task<T?> Get(string key)
        {
            _storage.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task Set(string key, T value)
        {
            _storage.AddOrUpdate(key, value, (k, v) => value);
            return Task.CompletedTask;
        }
    }

    public interface ITokenCounter
    {
        int CountTokens(string message);
    }

    public class TokenCounter : ITokenCounter
    {
        public int CountTokens(string message) => GPT3Tokenizer.Encode(message).Count;
    }

    public class ResultHistory
    {
        private readonly HashSet<string> _purchase = new();
        private readonly HashSet<string> _make = new();

        public ResultHistory(string chatId, IEnumerable<string> purchase, IEnumerable<string> make)
        {
            ChatId = chatId;
            _purchase = new HashSet<string>(purchase, StringComparer.InvariantCultureIgnoreCase);
            _make = new HashSet<string>(make, StringComparer.InvariantCultureIgnoreCase);
        }

        public ResultHistory(string chatId)
            : this(chatId, Enumerable.Empty<string>(), Enumerable.Empty<string>())
        {
        }

        public IReadOnlyCollection<string> Purchase => _purchase;

        public IReadOnlyCollection<string> Make => _make;

        public string ChatId { get; }

        public bool AddPurchase(string item) => _purchase.Add(item);

        public bool AddMake(string item) => _make.Add(item);
    }

    public class ResultRepository
    {
        private readonly IStorageContext<ResultHistory> _storage;
        public ResultRepository(IStorageContext<ResultHistory> storage) => _storage = storage;

        public async Task<ResultHistory> GetAsync(string chatId)
        {
            ResultHistory? history = await _storage.Get(chatId);

            if (history is null)
            {
                history = new ResultHistory(chatId);
                bool success = await _storage.Create(chatId, history);

                if (!success)
                {
                    history = await _storage.Get(chatId);

                    if (history is null)
                    {
                        throw new ApplicationException("Could not create result history");
                    }
                }
            }

            return history!;
        }

        public async Task StashAsync(ResultHistory resultHistory) => await _storage.Set(resultHistory.ChatId, resultHistory);
    }

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

    public class  ChatHistory : IReadOnlyList<string>
    {
        private readonly List<string> _messages = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHistory"/> class."/>
        /// </summary>
        /// <param name="chatId">The chat ID.</param>
        public ChatHistory(string chatId) => ChatId = chatId;

        /// <summary>
        /// Gets the chat ID.
        /// </summary>
        public string ChatId { get; init; }

        /// <inheritdoc/>
        public string this[int index] => _messages[index];

        /// <inheritdoc/>
        public int Count => _messages.Count;

        public void Add(string role, string content) => _messages.Add($"{role}: {content}");

        public string ConcatMessageHistory(ITokenCounter tokenCounter, int budget, Func<string, bool> filter = null)
        {
            filter = filter ?? (x => true);
            StringBuilder sb = new StringBuilder(budget);
            int spent = 0;

            string sep = string.Empty;

            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (!filter(_messages[i]))
                {
                    continue;
                }

                int cost = tokenCounter.CountTokens(_messages[i]);
                if (spent + cost > budget)
                {
                    break;
                }

                sb.Insert(0, sep).Insert(0, _messages[i]);
                sep = Environment.NewLine;

                spent += cost;
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public IEnumerator<string> GetEnumerator() => _messages.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
