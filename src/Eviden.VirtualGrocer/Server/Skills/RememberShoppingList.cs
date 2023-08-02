using Eviden.VirtualGrocer.Web.Server.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public class RememberShoppingList
    {
        [SKFunction("Remember the shopping list from the Personal Shopper skill.")]
        [SKFunctionName("Store")]
        public string Store(string shoppingList, SKContext context)
        {
            PersonalShopperCompletionResult result =
				System.Text.Json.JsonSerializer.Deserialize<PersonalShopperCompletionResult>(shoppingList)!;
            string json = System.Text.Json.JsonSerializer.Serialize(result);
            context.Variables["shoppingList"] = json;
            return json;
        }

        [SKFunction("Recall the shopping list from the Personal Shopper skill.")]
        [SKFunctionName("Recall")]
        [SKFunctionContextParameter(Name = "shoppingList")]
        public string Recall(SKContext context) => context.Variables["shoppingList"];
    }
}
