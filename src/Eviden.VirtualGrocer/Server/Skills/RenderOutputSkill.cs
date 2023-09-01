using System.ComponentModel;
using System.Text.Json;
using Eviden.VirtualGrocer.Shared.Models;
using Eviden.VirtualGrocer.Web.Server.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public class RenderOutputSkill
    {
        private readonly string _imagePath;
        public RenderOutputSkill(string imagePath)
        {
            _imagePath = imagePath;
        }

        [SKFunction]
        [SKName(SkillNames.RenderShoppingListResponse)]
        [Description("Render the output from the Personal Shopper skill.")]
        [SKParameter("products", "List of matching inventory items/products in shopping list")]
        [SKParameter("shoppingList", "List of desired products/recipes")]
        public string RenderShoppingListResponse(SKContext context)
        {
            string shoppingListOutput = context.Variables["shoppingList"];
            PersonalShopperCompletionResult result = JsonSerializer.Deserialize<PersonalShopperCompletionResult>(shoppingListOutput)!;
            var products = BuildProducts(context.Variables["products"]).ToArray();
            var recipes = result.Recipes.Select(x => (Recipe)x).ToArray();

            RenderOutputResult output = (recipes.Any(), products.Any(), !string.IsNullOrEmpty(result.Message)) switch
            {
                (false, false, true) => new ChatMessage { ErrorMessage = result.Message, IsError = true },
                (false, false, _) => new ChatMessage { InventoryContent = "We don't have any of the required ingredients in stock", PreContent = result.Message },
                (false, true, _) => new ChatMessage { InventoryContent = "These are items we have in stock related to your ask.", Products = products, PreContent = result.Message },
                (true, false, _) => new ChatMessage { RecipeContent = "Here are some recipe details", Recipes = recipes, InventoryContent = "We don't have any of the required ingredients in stock", PreContent = result.Message },
                (true, true, _) => new ChatMessage { RecipeContent = "Here are some recipe details", Recipes = recipes, InventoryContent = "These are items we have in stock related to your ask.", Products = products, PreContent = result.Message },
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
