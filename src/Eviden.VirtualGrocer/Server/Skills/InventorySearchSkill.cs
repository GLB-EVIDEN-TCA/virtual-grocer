using Azure.Search.Documents;
using Eviden.VirtualGrocer.Shared.Models;
using Eviden.VirtualGrocer.Web.Server.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.Text.Json;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public class InventorySearchSkill
    {
        private readonly SearchClient _searchClient;

        public InventorySearchSkill(SearchClient searchClient) =>
            _searchClient = searchClient;

        [SKFunction("Search inventory")]
        [SKFunctionName(SkillNames.FindInventory)]
        public async Task<string> FindInventory(string query, SKContext context)
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

        [SKFunction("Build a search query from the input.")]
        [SKFunctionContextParameter(Name = "shoppingList")]
        [SKFunctionName(SkillNames.BuildInventoryQuery)]
        public string BuildInventoryQuery(SKContext context)
        {
            var itemJson = context.Variables["shoppingList"];
            var shoppingList = JsonSerializer.Deserialize<PersonalShopperCompletionResult>(itemJson);

            IEnumerable<string> terms =
                shoppingList!.Recipes
                .SelectMany(x => (x.Ingredients ?? Enumerable.Empty<Ingredient>()).Select(y => y.Name))
                .Concat(shoppingList.ShoppingListItems)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => BuildQueryForItem(x!));

            var query = string.Join(" or ", terms);
            return query;
        }

        private static string BuildQueryForItem(string item) => $"\"{item}\"~";
    }
}