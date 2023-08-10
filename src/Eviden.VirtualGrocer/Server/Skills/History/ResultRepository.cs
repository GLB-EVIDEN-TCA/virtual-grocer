namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
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
}
