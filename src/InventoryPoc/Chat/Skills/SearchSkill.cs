using Azure.Search.Documents;
using System.Text.Json;

namespace InventoryPoc.Chat.Skills
{
    internal class  InventorySkill
    {
        private readonly SearchClient _searchClient;

        public InventorySkill(SearchClient searchClient)
        {
            _searchClient = searchClient;
        }

        public async Task<string> LookupAsync(string query)
        {
            var response = await _searchClient.SearchAsync<InventoryItem>(query);
            var results = await response.Value.GetResultsAsync().ToListAsync();
            return JsonSerializer.Serialize(results);
        }
    }

    public record InventoryItem
    {
        public string? Name { get; set; }
    }
}
