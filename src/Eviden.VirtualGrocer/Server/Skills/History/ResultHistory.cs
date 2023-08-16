namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
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
}
