using System.Text.Json;
using InventoryPoc.Web.Server.Models;
using InventoryPoc.Web.Shared.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace InventoryPoc.Web.Server.Skills
{
    public class RenderOutput
    {
        private readonly string _imagePath;
        public RenderOutput(string imagePath) { _imagePath = imagePath; }

        [SKFunction("Render the output from the Personal Shopper skill.")]
        [SKFunctionContextParameter(Name = "shoppingList")]
        [SKFunctionName("Render")]
        public string Render(SKContext context)
        {
            string shoppingListOutput = context.Variables["shoppingList"];
            PersonalShopperCompletionResult result = JsonSerializer.Deserialize<PersonalShopperCompletionResult>(shoppingListOutput)!;
            var products = BuildProducts(context.Variables["products"]).ToArray();
            var recipes = result.Recipes.Select(x => (Recipe)x).ToArray();

            RenderOutputResult output = (recipes.Any(), products.Any(), result.OtherContent.Any()) switch
            {
                (false, false, true) => new ChatMessage { PreContent = result.OtherContent.First(), IsError = true },
                (false, false, _) => new ChatMessage() { InventoryContent = "We don't have any of the required ingredients in stock" },
                (false, true, _) => new ChatMessage() { InventoryContent = "These are items we have in stock related to your ask.", Products = products },
                (true, false, _) => new ChatMessage() { RecipeContent = "Here are some recipe details", Recipes = recipes, InventoryContent = "We don't have any of the required ingredients in stock" },
                (true, true, _) => new ChatMessage() { RecipeContent = "Here are some recipe details", Recipes = recipes, InventoryContent = "These are items we have in stock related to your ask.", Products = products },
            };

            return output;
        }

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
