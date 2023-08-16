using System.Collections;
using System.Text;

namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
    public class ChatHistory : IReadOnlyList<string>
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

        public string ConcatMessageHistory(ITokenCounter tokenCounter, int budget, Func<string, bool>? filter = null)
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
