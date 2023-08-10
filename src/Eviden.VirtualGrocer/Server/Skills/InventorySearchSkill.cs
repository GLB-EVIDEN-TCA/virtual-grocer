using Azure.Search.Documents;
using Eviden.VirtualGrocer.Shared.Models;
using Eviden.VirtualGrocer.Web.Server.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;
using System.Text.Json;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public class InventorySearchSkill
    {
        private readonly SearchClient _searchClient;

        public InventorySearchSkill(SearchClient searchClient) =>
            _searchClient = searchClient;

        [SKFunction]
        [SKName(SkillNames.FindInventory)]
        [Description("Search inventory")]
        public async Task<string> FindInventory(string query, SKContext context)
        {
            if (string.IsNullOrEmpty(query))
            {
                context.Variables["products"] = "[]";
                return string.Empty;
            }

            var response = await _searchClient.SearchAsync<ProductSearchResult>(query);
            var results = response.Value.GetResultsAsync();

            List<ProductSearchResult> products = new List<ProductSearchResult>();
            await foreach (var result in results)
            {
                products.Add(result.Document);
            }

            string productString = JsonSerializer.Serialize(products);
            context.Variables["products"] = productString;
            return productString;
        }

        [SKFunction]
        [SKName(SkillNames.BuildInventoryQuery)]
        [Description("Build a search query from the input.")]
        [SKParameter("shoppingList", "The shopping list result")]
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