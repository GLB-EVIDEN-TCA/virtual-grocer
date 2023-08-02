using Azure.Search.Documents;
using Eviden.VirtualGrocer.Web.Server.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.Text.Json;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public class InventoryLookupSkill
    {
        private readonly SearchClient _searchClient;

        public InventoryLookupSkill(SearchClient searchClient) =>
            _searchClient = searchClient;

        [SKFunction("Search inventory")]
        [SKFunctionName("Lookup")]
        public async Task<string> LookupAsync(string query, SKContext context)
        {
            if (string.IsNullOrEmpty(query))
            {
                context["products"] = "[]";
                return string.Empty;
            }

            var response = await _searchClient.SearchAsync<ProductSearchResult>(query);
            var results = (await response.Value.GetResultsAsync().ToListAsync()).Select(x => x.Document);

            string products = JsonSerializer.Serialize(results);
            context["products"] = products;
            return products;
        }
    }
}