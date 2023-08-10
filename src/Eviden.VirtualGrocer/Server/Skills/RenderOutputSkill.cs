using System.ComponentModel;
using System.Text.Json;
using Eviden.VirtualGrocer.Shared.Models;
using Eviden.VirtualGrocer.Web.Server.Models;
using Eviden.VirtualGrocer.Web.Server.Skills.History;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public class RenderOutputSkill
    {
        private readonly string _imagePath;
        private readonly ResultRepository _resultRepo;
        public RenderOutputSkill(ResultRepository resultRepo, string imagePath)
        {
            _resultRepo = resultRepo;
            _imagePath = imagePath;
        }

        [SKFunction]
        [SKName(SkillNames.RenderShoppingListResponse)]
        [Description("Render the output from the Personal Shopper skill.")]
        [SKParameter("chatId", "Chat ID to extract history from")]
        [SKParameter("products", "List of matching inventory items/products in shopping list")]
        [SKParameter("shoppingList", "List of desired products/recipes")]
        public string RenderShoppingListResponse(SKContext context)
        {
            string shoppingListOutput = context.Variables["shoppingList"];
            PersonalShopperCompletionResult result = JsonSerializer.Deserialize<PersonalShopperCompletionResult>(shoppingListOutput)!;
            var products = BuildProducts(context.Variables["products"]).ToArray();
            var recipes = result.Recipes.Select(x => (Recipe)x).ToArray();
            var chatId = context.Variables["chatId"];

            RenderOutputResult output = (recipes.Any(), products.Any(), result.OtherContent.Any()) switch
            {
                (false, false, true) => new ChatMessage(chatId) { PreContent = result.OtherContent.First(), IsError = true },
                (false, false, _) => new ChatMessage(chatId) { InventoryContent = "We don't have any of the required ingredients in stock" },
                (false, true, _) => new ChatMessage(chatId) { InventoryContent = "These are items we have in stock related to your ask.", Products = products },
                (true, false, _) => new ChatMessage(chatId) { RecipeContent = "Here are some recipe details", Recipes = recipes, InventoryContent = "We don't have any of the required ingredients in stock" },
                (true, true, _) => new ChatMessage(chatId) { RecipeContent = "Here are some recipe details", Recipes = recipes, InventoryContent = "These are items we have in stock related to your ask.", Products = products },
            };

            return output;
        }

        [SKFunction]
        [SKName(SkillNames.RenderItemIntentResponse)]
        [Description("Render the output from the ItemIntent skill into a query for the PersonalShopper skill.")]
        [SKParameter("chatId", "Chat ID to extract history from")]
        [SKParameter("originalPrompt", "The original user prompt")]
        public async Task<string> RenderItemIntentResponse(string input, SKContext context)
        {
            ItemIntentCompletionResponse intent = JsonSerializer.Deserialize<ItemIntentCompletionResponse>(input)!;
            string chatId = context.Variables["chatId"];

            ResultHistory history = await _resultRepo.GetAsync(chatId);
            List<string> newPurchase = (intent.purchase ?? Array.Empty<string>()).Where(x => history.AddPurchase(x)).ToList();
            List<string> newMake = (intent.make ?? Array.Empty<string>()).Where(x => history.AddMake(x)).ToList();

            string query = $"{CompileItemIntent("I want to purchase", newPurchase)} {CompileItemIntent("I want to make", newMake)}".Trim();

            return !string.IsNullOrEmpty(query) ? query : context.Variables["originalPrompt"];
        }

        private string CompileItemIntent(string prefix, IReadOnlyCollection<string> items) =>
            items switch
            {
                { Count: 0 } => string.Empty,
                _ => $"{prefix} {string.Join(", ", items)}."
            };

        private record ItemIntentCompletionResponse(
            IReadOnlyCollection<string> purchase,
            IReadOnlyCollection<string> make,
            IReadOnlyCollection<string> other);

        private IEnumerable<Product> BuildProducts(string input)
        {
            IEnumerable<ProductSearchResult> products =
                !string.IsNullOrEmpty(input)
                    ? JsonSerializer.Deserialize<IEnumerable<ProductSearchResult>>(input)!
                    : new List<ProductSearchResult>();

            foreach (var item in products)
            {
                yield return item.ToProduct(_imagePath);
            }
        }

        // this is just an interim result class so the pattern-matching switch above looks a bit tidier.
        private class RenderOutputResult
        {
            private readonly string _value;
            public RenderOutputResult()
                : this("{ }")
            {
            }

            private RenderOutputResult(string value) => _value = value;

            public static implicit operator RenderOutputResult(string value) => new RenderOutputResult(value);
            public static implicit operator string(RenderOutputResult result) => result._value;
            public static implicit operator RenderOutputResult(ChatMessage message) =>
                new RenderOutputResult(JsonSerializer.Serialize(message));
        }
    }
}
